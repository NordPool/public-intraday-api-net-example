using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using Google.Protobuf;
using NPS.ID.PublicApi.BinaryApiClient.Cli;

namespace NPS.ID.PublicApi.BinaryApiClient.Connection;

/// <summary>
/// Shared WebSocket plumbing for the binary (protobuf) endpoints.
/// Handles connect → receive loop → token refresh → reconnect with exponential back-off.
/// Derived classes supply the URI, the frame parser/dispatcher and the token-refresh frame.
/// </summary>
public abstract class WebSocketConnection(string logPrefix, SsoClient ssoClient)
{
    private static readonly TimeSpan MaxBackoff = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan TokenRefreshBuffer = TimeSpan.FromMinutes(5);

    private ClientWebSocket? _ws;
    private int _attempt;

    /// <summary>Short prefix used on every log line, e.g. "PMD" or "TRD".</summary>
    public string LogPrefix { get; } = logPrefix;

    /// <summary>True while the WebSocket is open and frames can be sent.</summary>
    public bool IsConnected => _ws?.State == WebSocketState.Open;

    // -------------------------------------------------------------------------
    // Abstract / virtual hooks
    // -------------------------------------------------------------------------

    /// <summary>Builds the connection URI. <paramref name="isReconnect"/> is true for every attempt after the first.</summary>
    protected abstract Uri BuildUri(string sessionId, bool isReconnect);

    /// <summary>Parses and handles one complete binary server frame.</summary>
    protected abstract Task OnFrameAsync(MemoryStream frame, CancellationToken ct);

    /// <summary>Builds the ClientFrame carrying a refreshed access token.</summary>
    protected abstract IMessage BuildTokenRefreshFrame(string token);

    /// <summary>
    /// Optional extra loops to run for the lifetime of a connection (e.g. a heartbeat sender).
    /// The connection is torn down as soon as any returned task completes.
    /// </summary>
    protected virtual IEnumerable<Task> StartConnectionLoops(CancellationToken ct) => [];

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Runs the connection loop: connects, processes frames, and reconnects on failure.
    /// Stops cleanly when <paramref name="ct"/> is cancelled.
    /// </summary>
    public async Task RunAsync(CancellationToken ct)
    {
        var backoff = TimeSpan.FromSeconds(1);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var token = await ssoClient.GetToken();
                await ConnectAndRunAsync(token, ct);
                backoff = TimeSpan.FromSeconds(1); // reset on clean exit
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogError("ERROR", ex.Message);
                Log("RECONNECT", $"Waiting {backoff.TotalSeconds:0}s before reconnecting...");
                try { await Task.Delay(backoff, ct); } catch (OperationCanceledException) { break; }
                backoff = backoff * 2 < MaxBackoff ? backoff * 2 : MaxBackoff;
            }
        }

        Log("INFO", "Connection stopped.");
    }

    /// <summary>Gracefully closes the WebSocket (status 1000) if it is open.</summary>
    public async Task CloseAsync(string reason, CancellationToken ct)
    {
        var ws = _ws;
        if (ws is { State: WebSocketState.Open })
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, ct);
    }

    // -------------------------------------------------------------------------
    // Sending
    // -------------------------------------------------------------------------

    /// <summary>
    /// Serialises and sends a protobuf frame. Returns false (and logs) when not connected.
    /// </summary>
    protected async Task<bool> SendAsync(IMessage frame, CancellationToken ct)
    {
        var ws = _ws;
        if (ws is not { State: WebSocketState.Open })
        {
            LogError("SEND", "Not connected — frame dropped.");
            return false;
        }

        await ws.SendAsync(frame.ToByteArray(), WebSocketMessageType.Binary, endOfMessage: true, ct);
        return true;
    }

    // -------------------------------------------------------------------------
    // Connection loop
    // -------------------------------------------------------------------------

    private async Task ConnectAndRunAsync(string token, CancellationToken ct)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var uri = BuildUri(sessionId, isReconnect: _attempt > 0);
        _attempt++;

        using var webSocket = new ClientWebSocket();
        webSocket.Options.SetRequestHeader("Authorization", $"Bearer {token}");

        // Enable permessage-deflate compression as recommended by the spec.
        webSocket.Options.DangerousDeflateOptions = new WebSocketDeflateOptions();

        Log("CONNECT", uri.ToString());
        await webSocket.ConnectAsync(uri, ct);
        _ws = webSocket;
        Log("CONNECT", "WebSocket open");

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var receiveTask = ReceiveLoopAsync(webSocket, linkedCts.Token);
        var refreshTask = TokenRefreshLoopAsync(token, linkedCts.Token);
        var tasks = new List<Task> { receiveTask, refreshTask };
        tasks.AddRange(StartConnectionLoops(linkedCts.Token));

        try
        {
            // Stop all loops as soon as any finishes (connection closed or a loop failed).
            await Task.WhenAny(tasks);
            await linkedCts.CancelAsync();

            // Surface any exception from the receive loop.
            await receiveTask;
        }
        finally
        {
            _ws = null;
        }
    }

    // -------------------------------------------------------------------------
    // Token refresh loop
    // -------------------------------------------------------------------------

    private async Task TokenRefreshLoopAsync(string initialToken, CancellationToken ct)
    {
        var currentToken = initialToken;

        while (!ct.IsCancellationRequested)
        {
            var delay = GetTimeUntilRefresh(currentToken);
            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

            Log("TOKEN", $"Next refresh in {delay.TotalMinutes:0.#} min");
            await Task.Delay(delay, ct);

            try
            {
                Log("TOKEN", "Refreshing token...");
                var newToken = await ssoClient.GetToken();
                await SendAsync(BuildTokenRefreshFrame(newToken), ct);
                currentToken = newToken;
                Log("TOKEN", "Token refreshed successfully");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogError("TOKEN", $"Token refresh failed: {ex.Message}");
                throw;
            }
        }
    }

    private static TimeSpan GetTimeUntilRefresh(string token)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var refreshAt = jwt.ValidTo - TokenRefreshBuffer;
        return refreshAt - DateTime.UtcNow;
    }

    // -------------------------------------------------------------------------
    // Receive loop
    // -------------------------------------------------------------------------

    private async Task ReceiveLoopAsync(ClientWebSocket ws, CancellationToken ct)
    {
        var buffer = new byte[64 * 1024];
        using var message = new MemoryStream();

        while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            message.SetLength(0);
            WebSocketReceiveResult result;

            do
            {
                result = await ws.ReceiveAsync(buffer, ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Log("CLOSE", $"{result.CloseStatus} — {result.CloseStatusDescription}");
                    if (ws.State == WebSocketState.CloseReceived)
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Acknowledged", CancellationToken.None);
                    return;
                }

                message.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            if (result.MessageType != WebSocketMessageType.Binary)
                continue;

            message.Position = 0;
            await OnFrameAsync(message, ct);
        }
    }

    // -------------------------------------------------------------------------
    // Logging
    // -------------------------------------------------------------------------

    protected void Log(string tag, string msg) =>
        ConsoleIO.Write($"[{LogPrefix}] [{tag,-11}] {msg}");

    protected void LogError(string tag, string msg) =>
        ConsoleIO.WriteError($"[{LogPrefix}] [{tag,-11}] {msg}");
}
