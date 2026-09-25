using Google.Protobuf;
using NPS.ID.PublicApi.BinaryApiClient.Connection;
using NPS.Intraday.Edge.Trading.V2;

namespace NPS.ID.PublicApi.BinaryApiClient.Trading;

/// <summary>
/// Client for the binary Trading v2 API.
/// Subscriptions: order execution reports, private trades, throttling limits, configuration.
/// Requests: order entry, modify, deactivate. Every client frame carries a <c>request_id</c> that the
/// server echoes on the resulting TradingAck / first snapshot / error frames.
/// </summary>
public sealed class TradingClient(BinaryApiSettings settings, SsoClient ssoClient)
    : WebSocketConnection("TRD", ssoClient)
{
    private static readonly TimeSpan AutoHibeHeartbeatInterval = TimeSpan.FromSeconds(1);

    /// <summary>Latest order state per order, fed by the OER stream (populated after <c>oersub</c>).</summary>
    public OrderCache Orders { get; } = new();

    /// <summary>Latest trading profile of the user (populated after <c>cfgsub</c>).</summary>
    public ConfigurationCache Configuration { get; } = new();

    // -------------------------------------------------------------------------
    // Connection hooks
    // -------------------------------------------------------------------------

    protected override Uri BuildUri(string sessionId, bool isReconnect)
    {
        var url = string.Format(settings.Trading.Host, sessionId);
        var query = new List<string>();

        if (settings.Trading.AutoHibe) query.Add("autoHibe=true");
        // abortExisting is a recovery aid for unclean disconnects — only used when reconnecting.
        if (settings.Trading.AbortExisting && isReconnect) query.Add("abortExisting=true");

        return new Uri(query.Count == 0 ? url : $"{url}?{string.Join("&", query)}");
    }

    protected override IMessage BuildTokenRefreshFrame(string token) =>
        new ClientFrame { RequestId = NewRequestId(), TokenRefresh = new TokenRefresh { Token = token } };

    protected override IEnumerable<Task> StartConnectionLoops(CancellationToken ct)
    {
        // With autoHibe the server deactivates all orders after 10 s without a client heartbeat.
        if (settings.Trading.AutoHibe)
            yield return AutoHibeHeartbeatLoopAsync(ct);
    }

    private async Task AutoHibeHeartbeatLoopAsync(CancellationToken ct)
    {
        Log("AUTOHIBE", "Enabled — sending client heartbeat every 1s");
        using var timer = new PeriodicTimer(AutoHibeHeartbeatInterval);

        while (await timer.WaitForNextTickAsync(ct))
        {
            if (!IsConnected) return;
            await SendAsync(new ClientFrame { Heartbeat = new Heartbeat() }, ct);
        }
    }

    protected override Task OnFrameAsync(MemoryStream frame, CancellationToken ct)
    {
        var serverFrame = ServerFrame.Parser.ParseFrom(frame);
        var requestId = serverFrame.HasRequestId ? serverFrame.RequestId : null;

        switch (serverFrame.PayloadCase)
        {
            case ServerFrame.PayloadOneofCase.Connected:
            {
                var c = serverFrame.Connected;
                Log("CONNECTED",
                    $"heartbeat_interval_ms={c.HeartbeatIntervalMs} auto_hibe={c.AutoHibe} abort_existing={c.AbortExisting}");
                break;
            }

            case ServerFrame.PayloadOneofCase.Heartbeat:
                Log("HEARTBEAT", $"received {DateTimeOffset.UtcNow:HH:mm:ss}Z");
                break;

            case ServerFrame.PayloadOneofCase.Error:
                // Non-fatal errors keep the connection open; on fatal ones the server closes the socket
                // and the receive loop exits, triggering the reconnect loop.
                TradingPrinters.PrintError(requestId, serverFrame.Error);
                break;

            case ServerFrame.PayloadOneofCase.TradingAck:
                TradingPrinters.PrintTradingAck(requestId, serverFrame.TradingAck);
                break;

            case ServerFrame.PayloadOneofCase.OrderExecutionReport:
                Orders.Apply(serverFrame.OrderExecutionReport);
                TradingPrinters.PrintOrderExecutionReport(requestId, serverFrame.OrderExecutionReport);
                break;

            case ServerFrame.PayloadOneofCase.PrivateTrade:
                TradingPrinters.PrintPrivateTrade(requestId, serverFrame.PrivateTrade);
                break;

            case ServerFrame.PayloadOneofCase.ThrottlingLimit:
                TradingPrinters.PrintThrottlingLimit(serverFrame.ThrottlingLimit);
                break;

            case ServerFrame.PayloadOneofCase.Configuration:
                Configuration.Apply(serverFrame.Configuration);
                TradingPrinters.PrintConfiguration(requestId, serverFrame.Configuration);
                break;

            case ServerFrame.PayloadOneofCase.Statistic:
                Log("STATISTIC", $"rows={serverFrame.Statistic.Statistics.Count} (not subscribed by this example)");
                break;
        }

        return Task.CompletedTask;
    }

    // -------------------------------------------------------------------------
    // Heartbeat
    // -------------------------------------------------------------------------

    public async Task SendHeartbeatAsync(CancellationToken ct)
    {
        if (await SendAsync(new ClientFrame { Heartbeat = new Heartbeat() }, ct))
            Log("HEARTBEAT", "sent");
    }

    // -------------------------------------------------------------------------
    // Subscriptions
    // -------------------------------------------------------------------------

    public Task SubscribeOrderExecutionReportAsync(int? snapshotSize, CancellationToken ct)
    {
        var sub = new OrderExecutionReportSubscription();
        if (snapshotSize.HasValue) sub.SnapshotSize = snapshotSize.Value;
        return SendSubscribeAsync("OER-SUB", new Subscribe { OrderExecutionReport = sub },
            snapshotSize.HasValue ? $"snapshot_size={snapshotSize}" : "full snapshot", ct);
    }

    public Task UnsubscribeOrderExecutionReportAsync(CancellationToken ct) =>
        SendUnsubscribeAsync("OER-UNSUB", new Unsubscribe { OrderExecutionReport = new OrderExecutionReportSubscription() }, ct);

    public Task SubscribePrivateTradeAsync(int? snapshotSize, CancellationToken ct)
    {
        var sub = new PrivateTradeSubscription();
        if (snapshotSize.HasValue) sub.SnapshotSize = snapshotSize.Value;
        return SendSubscribeAsync("PT-SUB", new Subscribe { PrivateTrade = sub },
            snapshotSize.HasValue ? $"snapshot_size={snapshotSize}" : "full snapshot", ct);
    }

    public Task UnsubscribePrivateTradeAsync(CancellationToken ct) =>
        SendUnsubscribeAsync("PT-UNSUB", new Unsubscribe { PrivateTrade = new PrivateTradeSubscription() }, ct);

    public Task SubscribeThrottlingLimitAsync(CancellationToken ct) =>
        SendSubscribeAsync("TL-SUB", new Subscribe { ThrottlingLimit = new ThrottlingLimitsSubscription() }, "company limits", ct);

    public Task UnsubscribeThrottlingLimitAsync(CancellationToken ct) =>
        SendUnsubscribeAsync("TL-UNSUB", new Unsubscribe { ThrottlingLimit = new ThrottlingLimitsSubscription() }, ct);

    public Task SubscribeConfigurationAsync(CancellationToken ct) =>
        SendSubscribeAsync("CFG-SUB", new Subscribe { Configuration = new ConfigurationSubscription() }, "user profile", ct);

    public Task UnsubscribeConfigurationAsync(CancellationToken ct) =>
        SendUnsubscribeAsync("CFG-UNSUB", new Unsubscribe { Configuration = new ConfigurationSubscription() }, ct);

    private async Task SendSubscribeAsync(string tag, Subscribe subscribe, string description, CancellationToken ct)
    {
        var requestId = NewRequestId();
        if (await SendAsync(new ClientFrame { RequestId = requestId, Subscribe = subscribe }, ct))
            Log(tag, $"request_id={requestId} {description}");
    }

    private async Task SendUnsubscribeAsync(string tag, Unsubscribe unsubscribe, CancellationToken ct)
    {
        var requestId = NewRequestId();
        if (await SendAsync(new ClientFrame { RequestId = requestId, Unsubscribe = unsubscribe }, ct))
            Log(tag, $"request_id={requestId}");
    }

    // -------------------------------------------------------------------------
    // Trading requests
    // -------------------------------------------------------------------------

    public async Task SendOrderEntryAsync(OrderEntry entry, string clientOrderId, CancellationToken ct)
    {
        var order = entry.Orders[0];
        var description =
            $"ENTRY market={entry.MarketType} portfolio={order.PortfolioId} area={order.DeliveryAreaId} " +
            $"contract={string.Join(",", order.ContractIds)} side={order.Side} type={order.Type} qty={order.Quantity} " +
            $"price={order.UnitPrice} tif={order.TimeInForce} state={order.State}" +
            (order.HasExpireTimeMs ? $" expire={Format.Time(order.ExpireTimeMs)}" : "") +
            $" client_order_id={clientOrderId}";

        if (await SendTradingRequestAsync(new TradingRequest { Entry = entry }, description, ct))
            Orders.LastEnteredClientOrderId = clientOrderId;
    }

    public Task SendModificationAsync(OrderModification modification, CancellationToken ct)
    {
        var description = modification.OpCase switch
        {
            OrderModification.OpOneofCase.Modify => "MODIFY " + string.Join("; ", modification.Modify.Items.Select(i =>
                $"{DescribeRef(i.Ref)} -> contract={i.Payload.ContractId} type={i.Payload.Type} qty={i.Payload.Quantity} " +
                $"price={i.Payload.UnitPrice} tif={i.Payload.TimeInForce}" +
                (i.Payload.HasExpireTimeMs ? $" expire={Format.Time(i.Payload.ExpireTimeMs)}" : ""))),
            OrderModification.OpOneofCase.Deactivate => "DEACTIVATE " + string.Join("; ", modification.Deactivate.Refs.Select(DescribeRef)),
            OrderModification.OpOneofCase.Activate => "ACTIVATE " + string.Join("; ", modification.Activate.Refs.Select(DescribeRef)),
            OrderModification.OpOneofCase.Delete => "DELETE " + string.Join("; ", modification.Delete.Refs.Select(DescribeRef)),
            _ => modification.OpCase.ToString()
        };

        return SendTradingRequestAsync(new TradingRequest { Modification = modification },
            $"{description} market={modification.MarketType}", ct);
    }

    private async Task<bool> SendTradingRequestAsync(TradingRequest request, string description, CancellationToken ct)
    {
        var requestId = NewRequestId();
        var sent = await SendAsync(new ClientFrame { RequestId = requestId, TradingRequest = request }, ct);
        if (sent) Log("REQUEST", $"request_id={requestId} {description}");
        return sent;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string NewRequestId() => Guid.NewGuid().ToString();

    private static string DescribeRef(OrderRef r) =>
        (r.IdCase == OrderRef.IdOneofCase.OrderId ? $"order_id={r.OrderId}" : $"client_order_id={r.ClientOrderId}") +
        $" portfolio={r.PortfolioId}" + (r.HasRevision ? $" rev={r.Revision}" : "");
}
