using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using Google.Protobuf;
using NPS.Intraday.PMD.V2;

namespace NPS.ID.PublicApi.BinaryApiClient;

/// <summary>
/// WebSocket client for the binary PMD v2 API.
/// Handles the full connection lifecycle: connect → handshake → subscribe →
/// receive data/heartbeats/errors → reconnect with exponential back-off.
/// </summary>
public sealed class BinaryApiClient(BinaryApiSettings settings, SsoClient ssoClient)
{
    private static readonly TimeSpan MaxBackoff = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan TokenRefreshBuffer = TimeSpan.FromMinutes(5);

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Runs the client loop: connects, processes frames, and reconnects on failure.
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
                await ConnectAndRunAsync(ct, token);
                backoff = TimeSpan.FromSeconds(1); // reset on clean exit
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Log("ERROR", ex.Message);
                Log("RECONNECT", $"Waiting {backoff.TotalSeconds:0}s before reconnecting...");
                try { await Task.Delay(backoff, ct); } catch (OperationCanceledException) { break; }
                backoff = backoff * 2 < MaxBackoff ? backoff * 2 : MaxBackoff;
            }
        }

        Log("INFO", "Client stopped.");
    }

    // ── Local View ─────────────────────────────────────────────────────────

    /// <summary>Sends a Subscribe frame for local view on the given delivery area IDs.</summary>
    public async Task SubscribeLocalViewAsync(ClientWebSocket ws, IEnumerable<int> areas, CancellationToken ct)
    {
        var areaList = areas.ToList();
        await SendAsync(ws, new ClientFrame
        {
            Subscribe = new Subscribe { LocalView = new LocalViewSubscription { DeliveryAreas = { areaList } } }
        }, ct);
        Log("LV-SUB", $"areas=[{string.Join(", ", areaList)}]");
    }

    /// <summary>Sends an Unsubscribe frame for local view.
    /// Pass an empty list to unsubscribe from all areas.</summary>
    public async Task UnsubscribeLocalViewAsync(ClientWebSocket ws, IEnumerable<int> areas, CancellationToken ct)
    {
        var areaList = areas.ToList();
        await SendAsync(ws, new ClientFrame
        {
            Unsubscribe = new Unsubscribe { LocalView = new LocalViewSubscription { DeliveryAreas = { areaList } } }
        }, ct);
        Log("LV-UNSUB", areaList.Count == 0 ? "all areas" : $"areas=[{string.Join(", ", areaList)}]");
    }

    // ── Delivery Area ───────────────────────────────────────────────────────

    /// <summary>Sends a Subscribe frame for delivery areas (global).</summary>
    public async Task SubscribeDeliveryAreaAsync(ClientWebSocket ws, CancellationToken ct)
    {
        await SendAsync(ws, new ClientFrame
        {
            Subscribe = new Subscribe { DeliveryArea = new DeliveryAreaSubscription() }
        }, ct);
        Log("DA-SUB", "subscribed");
    }

    /// <summary>Sends an Unsubscribe frame for delivery areas.</summary>
    public async Task UnsubscribeDeliveryAreaAsync(ClientWebSocket ws, CancellationToken ct)
    {
        await SendAsync(ws, new ClientFrame
        {
            Unsubscribe = new Unsubscribe { DeliveryArea = new DeliveryAreaSubscription() }
        }, ct);
        Log("DA-UNSUB", "unsubscribed");
    }

    // ── Contract ────────────────────────────────────────────────────────────

    /// <summary>Sends a Subscribe frame for contracts (global).</summary>
    public async Task SubscribeContractAsync(ClientWebSocket ws, CancellationToken ct)
    {
        await SendAsync(ws, new ClientFrame
        {
            Subscribe = new Subscribe { Contract = new ContractSubscription() }
        }, ct);
        Log("C-SUB", "subscribed");
    }

    /// <summary>Sends an Unsubscribe frame for contracts.</summary>
    public async Task UnsubscribeContractAsync(ClientWebSocket ws, CancellationToken ct)
    {
        await SendAsync(ws, new ClientFrame
        {
            Unsubscribe = new Unsubscribe { Contract = new ContractSubscription() }
        }, ct);
        Log("C-UNSUB", "unsubscribed");
    }

    // ── Ticker ──────────────────────────────────────────────────────────────

    /// <summary>Sends a Subscribe frame for ticker / public trades (global).</summary>
    public async Task SubscribeTickerAsync(ClientWebSocket ws, CancellationToken ct)
    {
        await SendAsync(ws, new ClientFrame
        {
            Subscribe = new Subscribe { Ticker = new TickerSubscription() }
        }, ct);
        Log("T-SUB", "subscribed");
    }

    /// <summary>Sends an Unsubscribe frame for ticker.</summary>
    public async Task UnsubscribeTickerAsync(ClientWebSocket ws, CancellationToken ct)
    {
        await SendAsync(ws, new ClientFrame
        {
            Unsubscribe = new Unsubscribe { Ticker = new TickerSubscription() }
        }, ct);
        Log("T-UNSUB", "unsubscribed");
    }

    // ── Public Statistics ───────────────────────────────────────────────────

    /// <summary>Sends a Subscribe frame for public statistics on the given delivery area IDs.</summary>
    public async Task SubscribePublicStatisticAsync(ClientWebSocket ws, IEnumerable<int> areas, CancellationToken ct)
    {
        var areaList = areas.ToList();
        await SendAsync(ws, new ClientFrame
        {
            Subscribe = new Subscribe { PublicStatistic = new PublicStatisticSubscription { DeliveryAreas = { areaList } } }
        }, ct);
        Log("PS-SUB", $"areas=[{string.Join(", ", areaList)}]");
    }

    /// <summary>Sends an Unsubscribe frame for public statistics.
    /// Pass an empty list to unsubscribe from all areas.</summary>
    public async Task UnsubscribePublicStatisticAsync(ClientWebSocket ws, IEnumerable<int> areas, CancellationToken ct)
    {
        var areaList = areas.ToList();
        await SendAsync(ws, new ClientFrame
        {
            Unsubscribe = new Unsubscribe { PublicStatistic = new PublicStatisticSubscription { DeliveryAreas = { areaList } } }
        }, ct);
        Log("PS-UNSUB", areaList.Count == 0 ? "all areas" : $"areas=[{string.Join(", ", areaList)}]");
    }

    // ── H2H Capacity ────────────────────────────────────────────────────────

    /// <summary>Sends a Subscribe frame for H2H capacity on the given flow requests.</summary>
    public async Task SubscribeH2HCapacityAsync(ClientWebSocket ws, IEnumerable<CapacityFlow> requests, CancellationToken ct)
    {
        var requestList = requests.ToList();
        await SendAsync(ws, new ClientFrame
        {
            Subscribe = new Subscribe { H2HCapacity = new H2HCapacitySubscription { Flows = { requestList } } }
        }, ct);
        Log("H2H-SUB", FormatCapacityRequests(requestList));
    }

    /// <summary>Sends an Unsubscribe frame for H2H capacity.
    /// Pass an empty list to unsubscribe from all H2H capacity areas.</summary>
    public async Task UnsubscribeH2HCapacityAsync(ClientWebSocket ws, IEnumerable<CapacityFlow> requests, CancellationToken ct)
    {
        var requestList = requests.ToList();
        await SendAsync(ws, new ClientFrame
        {
            Unsubscribe = new Unsubscribe { H2HCapacity = new H2HCapacitySubscription { Flows = { requestList } } }
        }, ct);
        Log("H2H-UNSUB", requestList.Count == 0 ? "all H2H capacity areas" : FormatCapacityRequests(requestList));
    }

    // ── ATC Capacity ────────────────────────────────────────────────────────

    /// <summary>Sends a Subscribe frame for ATC capacity on the given flow requests.</summary>
    public async Task SubscribeAtcCapacityAsync(ClientWebSocket ws, IEnumerable<CapacityFlow> requests, CancellationToken ct)
    {
        var requestList = requests.ToList();
        await SendAsync(ws, new ClientFrame
        {
            Subscribe = new Subscribe { AtcCapacity = new AtcCapacitiesSubscription { Flows = { requestList } } }
        }, ct);
        Log("ATC-SUB", FormatCapacityRequests(requestList));
    }

    /// <summary>Sends an Unsubscribe frame for ATC capacity.
    /// Pass an empty list to unsubscribe from all ATC capacity areas.</summary>
    public async Task UnsubscribeAtcCapacityAsync(ClientWebSocket ws, IEnumerable<CapacityFlow> requests, CancellationToken ct)
    {
        var requestList = requests.ToList();
        await SendAsync(ws, new ClientFrame
        {
            Unsubscribe = new Unsubscribe { AtcCapacity = new AtcCapacitiesSubscription { Flows = { requestList } } }
        }, ct);
        Log("ATC-UNSUB", requestList.Count == 0 ? "all ATC capacity areas" : FormatCapacityRequests(requestList));
    }

    // -------------------------------------------------------------------------
    // Connection loop
    // -------------------------------------------------------------------------

    private async Task ConnectAndRunAsync(CancellationToken ct, string token)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var uri = new Uri(string.Format(settings.Host, sessionId));
        using var webSocket = new ClientWebSocket();

        webSocket.Options.SetRequestHeader("Authorization", $"Bearer {token}");

        // Enable permessage-deflate compression as recommended by the spec.
        webSocket.Options.DangerousDeflateOptions = new WebSocketDeflateOptions();

        Log("CONNECT", uri.ToString());
        await webSocket.ConnectAsync(uri, ct);

        Log("CONNECT", "WebSocket open");

        // Run receive loop, interactive console input, and token refresh concurrently.
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var receiveTask = ReceiveLoopAsync(webSocket, linkedCts.Token);
        var inputTask   = ConsoleInputLoopAsync(webSocket, linkedCts.Token);
        var refreshTask = TokenRefreshLoopAsync(webSocket, token, linkedCts.Token);

        // Stop all tasks as soon as any finishes (connection closed or user quit).
        await Task.WhenAny(receiveTask, inputTask, refreshTask);
        await linkedCts.CancelAsync();

        // Surface any exception from the receive loop.
        await receiveTask;
    }

    // -------------------------------------------------------------------------
    // Token refresh loop
    // -------------------------------------------------------------------------

    private async Task TokenRefreshLoopAsync(ClientWebSocket ws, string initialToken, CancellationToken ct)
    {
        var currentToken = initialToken;

        while (!ct.IsCancellationRequested)
        {
            var delay = GetTimeUntilRefresh(currentToken);

            Log("TOKEN", $"Next refresh in {delay.TotalMinutes:0.#} min");
            await Task.Delay(delay, ct);

            try
            {
                Log("TOKEN", "Refreshing token...");
                var newToken = await ssoClient.GetToken();
                await SendAsync(ws, new ClientFrame
                {
                    TokenRefresh = new TokenRefresh { Token = newToken }
                }, ct);
                currentToken = newToken;
                Log("TOKEN", "Token refreshed successfully");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Log("TOKEN", $"Token refresh failed: {ex.Message}");
                throw;
            }
        }
    }

    private TimeSpan GetTimeUntilRefresh(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
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
            var frame = ServerFrame.Parser.ParseFrom(message);
            await HandleServerFrameAsync(ws, frame, ct);
        }
    }

    // -------------------------------------------------------------------------
    // Frame handlers
    // -------------------------------------------------------------------------

    private async Task HandleServerFrameAsync(ClientWebSocket ws, ServerFrame frame, CancellationToken ct)
    {
        switch (frame.PayloadCase)
        {
            case ServerFrame.PayloadOneofCase.Connected:
                Log("CONNECTED", $"heartbeat_interval_ms={frame.Connected.HeartbeatIntervalMs}");
                // Send initial subscription straight after handshake.
                // if (settings.Areas.Length > 0)
                //     await SubscribeAsync(ws, settings.Areas, ct);
                break;

            case ServerFrame.PayloadOneofCase.Heartbeat:
                Log("HEARTBEAT", FormatDateTime(0));
                break;

            case ServerFrame.PayloadOneofCase.Error:
                // Log error; if the server subsequently closes the socket, ReceiveLoopAsync will exit.
                LogError("ERROR", frame.Error.Message);
                break;

            case ServerFrame.PayloadOneofCase.LocalView:
                PrintLocalView(frame);
                break;

            case ServerFrame.PayloadOneofCase.DeliveryArea:
                PrintDeliveryArea(frame);
                break;

            case ServerFrame.PayloadOneofCase.Contract:
                PrintContract(frame);
                break;

            case ServerFrame.PayloadOneofCase.Ticker:
                PrintTicker(frame);
                break;

            case ServerFrame.PayloadOneofCase.H2HCapacity:
                PrintH2HCapacity(frame);
                break;

            case ServerFrame.PayloadOneofCase.PublicStatistic:
                PrintPublicStatistic(frame);
                break;

            case ServerFrame.PayloadOneofCase.AtcCapacity:
                PrintAtcCapacity(frame);
                break;
        }
    }

    private static void PrintLocalView(ServerFrame frame)
    {
        var kind = frame.LocalView.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new System.Text.StringBuilder();

        foreach (var lv in frame.LocalView.LocalViews)
        {
            sb.AppendLine($"[LV {kind}] area={frame.LocalView.DeliveryAreaId,-6} contract={lv.ContractId,-12} market={lv.MarketType,-20} rev={lv.RevisionNo}");

            if (lv.BuyOrders.Count > 0)
            {
                sb.AppendLine($"  Buy  ({lv.BuyOrders.Count}):");
                foreach (var o in lv.BuyOrders)  AppendOrder(sb, o);
            }

            if (lv.SellOrders.Count > 0)
            {
                sb.AppendLine($"  Sell ({lv.SellOrders.Count}):");
                foreach (var o in lv.SellOrders) AppendOrder(sb, o);
            }
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    private static void AppendOrder(System.Text.StringBuilder sb, Order o)
    {
        var status = o.Quantity == 0 ? "DELETED      " : $"qty={o.Quantity,-9}";
        sb.AppendLine($"    order={o.OrderId,-14} price={o.Price,-10} {status} ");
    }

    private static void PrintDeliveryArea(ServerFrame frame)
    {
        var kind = frame.DeliveryArea.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new System.Text.StringBuilder();

        foreach (var da in frame.DeliveryArea.DeliveryAreas)
        {
            var products = string.Join(", ", da.ProductTypes);
            sb.AppendLine(
                $"[DA {kind}] id={da.DeliveryAreaId,-4} eic={da.EicCode,-16} area={da.AreaCode,-6} " +
                $"currency={da.CurrencyCode,-4} tz={da.TimeZone,-20} country={da.CountryIsoCode,-3} products=[{products}]");
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    private static void PrintContract(ServerFrame frame)
    {
        var kind = frame.Contract.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new System.Text.StringBuilder();

        foreach (var c in frame.Contract.Contracts)
        {
            var deliveryStart = FormatDateTime(c.DeliveryStartMs);
            var deliveryEnd   = FormatDateTime(c.DeliveryEndMs);
            sb.AppendLine(
                $"[CTR {kind}] id={c.ContractId,-12} name={c.ContractName,-20} market={c.MarketType,-12} " +
                $"product={c.ProductType,-24} delivery={deliveryStart} - {deliveryEnd}");

            foreach (var state in c.DeliveryAreaStates)
            {
                sb.AppendLine(
                    $"    area={state.DeliveryAreaId,-4} state={state.State,-20} " +
                    $"open={FormatDateTime(state.OpenAtMs)} close={FormatDateTime(state.ClosedAtMs)}");
            }
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    private static void PrintTicker(ServerFrame frame)
    {
        var kind = frame.Ticker.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new System.Text.StringBuilder();

        foreach (var t in frame.Ticker.Trades)
        {
            var tradeTime = FormatDateTime(t.TradeTimeMs);
            sb.AppendLine(
                $"[TKR {kind}] id={t.TradeId,-12} market={t.MarketType,-12} state={t.State,-24} " +
                $"rev={t.RevisionNo} time={tradeTime}");

            foreach (var leg in t.Legs)
            {
                sb.AppendLine(
                    $"    contract={leg.ContractId,-12} side={leg.Side,-6} price={leg.UnitPrice,-10} " +
                    $"qty={leg.Quantity,-8} area={leg.DeliveryAreaId}");
            }
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    private static void PrintH2HCapacity(ServerFrame frame)
    {
        var kind = frame.H2HCapacity.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new System.Text.StringBuilder();

        foreach (var c in frame.H2HCapacity.H2HCapacities)
        {
            var from = FormatDateTime(c.DeliveryStartMs);
            var to   = FormatDateTime(c.DeliveryEndMs);
            var pub  = FormatDateTime(c.PublishedAtMs);

            sb.AppendLine(
                $"[H2H {kind}] area={frame.H2HCapacity.DeliveryAreaId,-4} {c.DeliveryAreaFrom}->{c.DeliveryAreaTo}  " +
                $"delivery={from} - {to}  in={c.InCapacity} out={c.OutCapacity}  " +
                $"published={pub} ");
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    private static void PrintPublicStatistic(ServerFrame frame)
    {
        var kind = frame.PublicStatistic.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new System.Text.StringBuilder();

        foreach (var ps in frame.PublicStatistic.PublicStatistics)
        {
            sb.AppendLine(
                $"[PS {kind}] area={ps.DeliveryAreaId,-4} market={ps.MarketType,-12} contract={ps.ContractId,-12} " +
                $"last={ps.LastPrice?.ToString() ?? "-"} qty={ps.LastQuantity?.ToString() ?? "-"} " +
                $"high={ps.HighestPrice?.ToString() ?? "-"} low={ps.LowestPrice?.ToString() ?? "-"} " +
                $"vwap={ps.Vwap?.ToString() ?? "-"} turnover={ps.Turnover?.ToString() ?? "-"} " +
                $"tendency={ps.Tendency}");
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    private static void PrintAtcCapacity(ServerFrame frame)
    {
        var kind = frame.AtcCapacity.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new System.Text.StringBuilder();

        foreach (var c in frame.AtcCapacity.AtcCapacities)
        {
            var from = FormatDateTime(c.DeliveryStartMs);
            var to   = FormatDateTime(c.DeliveryEndMs);
            var upd  = FormatDateTime(c.UpdatedAtMs);

            sb.AppendLine(
                $"[ATC {kind}] area={frame.AtcCapacity.DeliveryAreaId,-4} {c.FromDeliveryAreaId}->{c.ToDeliveryAreaId}  " +
                $"delivery={from} - {to}  capacity={c.Capacity}  updated={upd}");
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    // -------------------------------------------------------------------------
    // Interactive console input
    // -------------------------------------------------------------------------

    private async Task ConsoleInputLoopAsync(ClientWebSocket ws, CancellationToken ct)
    {
        PrintHelp();

        while (!ct.IsCancellationRequested && ws.State == WebSocketState.Open)
        {
            string? line;
            try
            {
                line = await ConsoleIO.ReadLineAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (line is null) break; // stdin closed

            var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0].ToLowerInvariant())
            {
                case "lvsub":
                {
                    var areas = ParseAreas(parts[1..]);
                    if (areas is null) { ConsoleIO.Write("Usage: lvsub <area1> [area2 ...]"); break; }
                    await SubscribeLocalViewAsync(ws, areas, ct);
                    break;
                }
                case "lvunsub":
                {
                    var areas = ParseAreas(parts[1..]) ?? [];
                    await UnsubscribeLocalViewAsync(ws, areas, ct);
                    break;
                }
                case "dasub":
                    await SubscribeDeliveryAreaAsync(ws, ct);
                    break;
                case "daunsub":
                    await UnsubscribeDeliveryAreaAsync(ws, ct);
                    break;
                case "csub":
                    await SubscribeContractAsync(ws, ct);
                    break;
                case "cunsub":
                    await UnsubscribeContractAsync(ws, ct);
                    break;
                case "tsub":
                    await SubscribeTickerAsync(ws, ct);
                    break;
                case "tunsub":
                    await UnsubscribeTickerAsync(ws, ct);
                    break;
                case "pssub":
                {
                    var areas = ParseAreas(parts[1..]);
                    if (areas is null) { ConsoleIO.Write("Usage: pssub <area1> [area2 ...]"); break; }
                    await SubscribePublicStatisticAsync(ws, areas, ct);
                    break;
                }
                case "psunsub":
                {
                    var areas = ParseAreas(parts[1..]) ?? [];
                    await UnsubscribePublicStatisticAsync(ws, areas, ct);
                    break;
                }
                case "h2hsub":
                {
                    var requests = ParseCapacityAreas(parts[1..]);
                    if (requests is null) { ConsoleIO.Write("Usage: h2hsub <area[:conn,conn,...]> [...]"); break; }
                    await SubscribeH2HCapacityAsync(ws, requests, ct);
                    break;
                }
                case "h2hunsub":
                {
                    var requests = ParseCapacityAreas(parts[1..]) ?? [];
                    await UnsubscribeH2HCapacityAsync(ws, requests, ct);
                    break;
                }
                case "atcsub":
                {
                    var requests = ParseCapacityAreas(parts[1..]);
                    if (requests is null) { ConsoleIO.Write("Usage: atcsub <area[:conn,conn,...]> [...]"); break; }
                    await SubscribeAtcCapacityAsync(ws, requests, ct);
                    break;
                }
                case "atcunsub":
                {
                    var requests = ParseCapacityAreas(parts[1..]) ?? [];
                    await UnsubscribeAtcCapacityAsync(ws, requests, ct);
                    break;
                }
                case "heartbeat" or "hb":
                    await SendAsync(ws, new ClientFrame { Heartbeat = new Heartbeat() }, ct);
                    Log("HEARTBEAT", "sent");
                    break;
                case "help" or "?":
                    PrintHelp();
                    break;
                case "quit" or "exit" or "q":
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "User quit", ct);
                    return;
                default:
                    ConsoleIO.Write($"Unknown command '{parts[0]}'. Type 'help' for usage.");
                    break;
            }
        }
    }

    private static void PrintHelp()
    {
        ConsoleIO.Write("""
Commands:
  lvsub <area1> [area2 ...]   Subscribe to local view for delivery areas
  lvunsub [area1 area2 ...]   Unsubscribe local view (no args = all areas)
  dasub                       Subscribe to delivery areas (global)
  daunsub                     Unsubscribe delivery areas
  csub                        Subscribe to contracts (global)
  cunsub                      Unsubscribe contracts
  tsub                        Subscribe to ticker / public trades (global)
  tunsub                      Unsubscribe ticker
  pssub <area1> [area2 ...]   Subscribe to public statistics for areas
  psunsub [area1 area2 ...]   Unsubscribe public statistics (no args = all)
  h2hsub <area[:conn,...]>    Subscribe to H2H capacity (e.g. h2hsub 10:11,12)
  h2hunsub [area1 area2 ..]   Unsubscribe H2H capacity (no args = all)
  atcsub <area[:conn,...]>    Subscribe to ATC capacity (e.g. atcsub 10:11,12)
  atcunsub [area1 area2 ..]   Unsubscribe ATC capacity (no args = all)
  hb                          Send a heartbeat frame
  quit                        Close connection and exit
  help                        Show this help
""");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static async Task SendAsync(ClientWebSocket ws, ClientFrame frame, CancellationToken ct)
    {
        var bytes = frame.ToByteArray();
        await ws.SendAsync(bytes, WebSocketMessageType.Binary, endOfMessage: true, ct);
    }

    private static int[]? ParseAreas(string[] tokens)
    {
        if (tokens.Length == 0) return null;
        var areas = new int[tokens.Length];
        for (var i = 0; i < tokens.Length; i++)
            if (!int.TryParse(tokens[i], out areas[i])) return null;
        return areas;
    }

    /// <summary>
    /// Parses capacity area tokens. Each token is "area" or "area:connected1,connected2,...".
    /// Examples: "10", "10 20", "10:11,12", "10:11,12 20:21,22"
    /// </summary>
    private static List<CapacityFlow>? ParseCapacityAreas(string[] tokens)
    {
        if (tokens.Length == 0) return null;
        var requests = new List<CapacityFlow>();
        foreach (var token in tokens)
        {
            var parts = token.Split(':', 2);
            if (!int.TryParse(parts[0], out var area)) return null;
            var connected = new List<int>();
            if (parts.Length == 2)
            {
                foreach (var p in parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!int.TryParse(p, out var c)) return null;
                    connected.Add(c);
                }
            }
            requests.Add(new CapacityFlow { DeliveryArea = area, ConnectedDeliveryAreas = { connected } });
        }
        return requests;
    }

    private static string FormatCapacityRequests(List<CapacityFlow> requests) =>
        string.Join("; ", requests.Select(r =>
            $"{r.DeliveryArea}->[{string.Join(",", r.ConnectedDeliveryAreas)}]"));

    private static string FormatDateTime(long ms) =>
        ms == 0 ? "-" : DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime.ToString("O");

    private static void Log(string tag, string msg) =>
        ConsoleIO.Write($"[{tag,-11}] {msg}");

    private static void LogError(string tag, string msg) =>
        ConsoleIO.Write($"[{tag,-11}] {msg}");
}
