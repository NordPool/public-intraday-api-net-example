using System.Buffers;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Nordpool.ID.PublicApi.v1.Command;
using NPS.ID.PublicApi.Client.Connection.Events;
using NPS.ID.PublicApi.Client.Connection.Exceptions;
using NPS.ID.PublicApi.Client.Connection.Messages;
using NPS.ID.PublicApi.Client.Connection.Options;
using NPS.ID.PublicApi.Client.Security;
using NPS.ID.PublicApi.Client.Security.Options;
using NPS.ID.PublicApi.Client.Connection.Extensions;

namespace NPS.ID.PublicApi.Client.Connection;

public class WebSocketConnector : IAsyncDisposable
{
    private const string SecureWebSocketProtocol = "wss";

    private bool _isConnected;
    public bool IsConnected => _isConnected && _webSocket.State == WebSocketState.Open;

    public string ConnectionUri { get; private set; }

    private readonly ILogger<WebSocketConnector> _logger;
    
    private readonly ISsoService _ssoService;

    private string _currentAuthToken;
    private readonly ClientWebSocket _webSocket = new();
    private readonly WebSocketOptions _webSocketOptions;
    private readonly CredentialsOptions _credentialsOptions;
    
    private readonly Func<Task> _connectionEstablishedCallbackAsync;
    private readonly Func<Task> _connectionClosedCallbackAsync;
    private readonly Func<MessageReceivedEventArgs, CancellationToken, Task> _messageReceivedCallbackAsync;
    private readonly Func<StompConnectionException, Task> _stompErrorCallbackAsync;

    private readonly CancellationTokenSource _connectorCancellationTokenSource = new();
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

    private readonly string _serverId;
    public WebSocketConnector(ILogger<WebSocketConnector> logger,
        ISsoService ssoService,
        WebSocketOptions webSocketOptions,
        CredentialsOptions credentialsOptions, 
        Func<MessageReceivedEventArgs, CancellationToken, Task> messageReceivedCallbackAsync,
        Func<Task> connectionEstablishedCallbackAsync = null,
        Func<Task> connectionClosedCallbackAsync = null,
        Func<StompConnectionException, Task> stompErrorCallbackAsync = null)
    {
        _logger = logger;
        _ssoService = ssoService;
        _webSocketOptions = webSocketOptions;
        _credentialsOptions = credentialsOptions;
        
        _messageReceivedCallbackAsync = messageReceivedCallbackAsync;
        _connectionEstablishedCallbackAsync = connectionEstablishedCallbackAsync;
        _connectionClosedCallbackAsync = connectionClosedCallbackAsync;
        _stompErrorCallbackAsync = stompErrorCallbackAsync;
        
        _serverId = ConstructServerId();
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _currentAuthToken = await _ssoService.GetAuthTokenAsync(_credentialsOptions.UserName, _credentialsOptions.Password,
            cancellationToken);
        
        var uri = ConstructUri();
        await _webSocket.ConnectAsync(uri, cancellationToken);
        
        _logger.LogDebug("Web socket opened");

        _ = Task.Run(() => StartReceivingMessagesAsync(_connectorCancellationTokenSource.Token), cancellationToken);
    }
    
    private Uri ConstructUri()
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var uri =
            $"{SecureWebSocketProtocol}://{_webSocketOptions.Host}:{_webSocketOptions.SslPort}{_webSocketOptions.Uri}/{_serverId}/{sessionId}/websocket";

        _logger.LogDebug("Connecting to: {uri}", uri);
        ConnectionUri = uri;
        return new Uri(uri);
    }
    
    private static string ConstructServerId()
    {
        var serverId = Random.Shared.Next(0, 999);

        return serverId.ToString().PadLeft(3, '0');
    }
    
    public async Task SendAsync(ArraySegment<byte> message, CancellationToken cancellationToken)
    {
        if (_webSocket.State != WebSocketState.Open)
        {
            _logger.LogWarning("Trying to send message {Message} while connection is in {WebSocketState} state", Encoding.UTF8.GetString(message), _webSocket.State);
            return;
        }

        await _webSocket.SendAsync(message, WebSocketMessageType.Text, true, cancellationToken);
    }

    private async Task StartReceivingMessagesAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        var receiveBuffer = ArrayPool<byte>.Shared.Rent(1024 * 1024);
        var message = new ReceivedMessage();

        try
        {
            while (_webSocket.State != WebSocketState.Closed && !cancellationToken.IsCancellationRequested)
            {
                var receiveResult = await _webSocket.ReceiveAsync(receiveBuffer, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (_webSocket.State == WebSocketState.CloseReceived &&
                    receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await CloseConnectionAsync(cancellationToken);
                    break;
                }

                if (_webSocket.State == WebSocketState.Open && receiveResult.MessageType != WebSocketMessageType.Close)
                {
                    message.Append(receiveBuffer, 0, receiveResult.Count, receiveResult.EndOfMessage);

                    if (receiveResult.EndOfMessage)
                    {
                        await HandleReceivedMessageAsync(message, cancellationToken);
                        message.Dispose();
                        message = new ReceivedMessage();
                    }
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("Stop application request received. Stopped receiving WebSocket messages.");
        }
        catch (Exception ex)
        {
            await CloseConnectionAsync(CancellationToken.None);
            _logger.LogWarning(ex, "An error on web socket message receiving");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(receiveBuffer);
            message.Dispose();
        }
    }
    
    private async Task HandleReceivedMessageAsync(ReceivedMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow;

            if (message.IsSockJsStart())
            {
                var connectCommand = StompMessageFactory.ConnectionFrame(_currentAuthToken, _webSocketOptions.HeartbeatOutgoingInterval);
                await SendAsync(connectCommand.ConvertToMessageBytes(), cancellationToken);
                return;
            }

            if (message.IsError())
            {
                await OnWebSocketErrorAsync(message);
                return;
            }

            if (message.IsHeartBeat())
            {
                return;
            }

            if (message.IsDisconnectCode())
            {
                return;
            }

            if (message.IsConnectedCommand())
            {
                _isConnected = true;
                await OnWebSocketConnectedAsync();
            }
            else
            {
                await OnMessageReceivedAsync(new MessageReceivedEventArgs { Message = message, Timestamp = timestamp }, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "An error on web socket message callback");
        }
    }

    private async Task OnWebSocketErrorAsync(ReceivedMessage message)
    {
        try
        {
            var stompFrame = message.ConvertToStompFrame();
            if (stompFrame.Properties.TryGetValue(Headers.Server.Message, out var errorMessage) && _stompErrorCallbackAsync is not null)
                await _stompErrorCallbackAsync.Invoke(new StompConnectionException(errorMessage));
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "An error on web socket error callback");
        }
    }
    
    private async Task CloseConnectionAsync(CancellationToken cancellationToken)
    {
        if (!_isConnected)
        {
            return;
        }

        _isConnected = false;

        switch (_webSocket.State)
        {
            case WebSocketState.Open:
                await SendDisconnectAsync(cancellationToken);
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                break;
            case WebSocketState.CloseReceived:
                _logger.LogInformation("Closing WebSocket due to `CloseReceived` state");
                await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                break;
            default:
                _logger.LogWarning("Unsupported ws state {State} when closing connection", _webSocket.State);
                break;
        }

        await _connectorCancellationTokenSource.CancelAsync();

        await OnWebSocketClosedAsync();
    }

    private Task SendDisconnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Sending Disconnect \"1000\"");
        return SendAsync(WebSocketMessages.DisconnectCode, cancellationToken);
    }
    
    private async Task OnWebSocketConnectedAsync()
    {
        _ = Task.Run(() => PeriodicallyRefreshTokenAsync(_connectorCancellationTokenSource.Token));
        _ = Task.Run(() => PeriodicallySendHeartBeatMessagesAsync(_connectorCancellationTokenSource.Token));

        try
        {
            if (_connectionEstablishedCallbackAsync is not null)
                await _connectionEstablishedCallbackAsync.Invoke();
        
            _logger.LogDebug("WebSocket Connection established");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "An error on web socket connected callback");
        }
    }
    
    private async Task PeriodicallyRefreshTokenAsync(CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(_currentAuthToken);

        while (IsConnected && !cancellationToken.IsCancellationRequested)
        {
            var jwt = _jwtSecurityTokenHandler.ReadToken(_currentAuthToken);
            var expirationDate = new DateTimeOffset(jwt.ValidTo, TimeSpan.Zero);
            var refreshPeriod = expirationDate.Subtract(DateTimeOffset.UtcNow.AddMinutes(5));

            using var timer = new PeriodicTimer(refreshPeriod);
            await timer.WaitForNextTickAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await RefreshAccessTokenAsync(cancellationToken);
        }
    }

    private async Task RefreshAccessTokenAsync(CancellationToken cancellationToken)
    {
        var previousAuthToken = _currentAuthToken;
        _currentAuthToken = await _ssoService.GetAuthTokenAsync(_credentialsOptions.UserName, _credentialsOptions.Password, cancellationToken);

        var refreshTokenCommand = new TokenRefreshCommand
        {
            NewToken = _currentAuthToken,
            OldToken = previousAuthToken
        };

        var refreshTokenCommandPayload = JsonSerializer.Serialize(refreshTokenCommand);
        
        var stompFrame = StompMessageFactory.SendFrame(refreshTokenCommandPayload, "/v1/command");

        _logger.LogDebug("Sending token refresh frame");

        await SendAsync(stompFrame.ConvertToMessageBytes(), cancellationToken);
    }

    private async Task PeriodicallySendHeartBeatMessagesAsync(CancellationToken cancellationToken)
    {
        if (_webSocketOptions.HeartbeatOutgoingInterval == 0)
        {
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_webSocketOptions.HeartbeatOutgoingInterval));
        while (IsConnected && !cancellationToken.IsCancellationRequested)
        {
            await timer.WaitForNextTickAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            _logger.LogDebug("Sending heartbeat frame");

            // must send an end-of-line as heartbeat frame
            await SendAsync(WebSocketMessages.ClientHeartBeat, cancellationToken);
        }
    }

    private async Task OnWebSocketClosedAsync()
    {
        try
        {
            if (_connectionClosedCallbackAsync is not null)
                await _connectionClosedCallbackAsync.Invoke();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error on web socket closed callback");
        }
    }

    private async Task OnMessageReceivedAsync(MessageReceivedEventArgs eventArgs, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Received new message: {Message}", eventArgs.Message);

        try
        {
            if (_messageReceivedCallbackAsync is not null)
                await _messageReceivedCallbackAsync.Invoke(eventArgs, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error on web socket message received callback");
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_isConnected)
        {
            await CloseConnectionAsync(CancellationToken.None);
        }

        _connectorCancellationTokenSource.Dispose();
        _webSocket.Dispose();
    }
}