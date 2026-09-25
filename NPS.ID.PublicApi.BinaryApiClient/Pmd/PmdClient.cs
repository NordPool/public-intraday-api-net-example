using Google.Protobuf;
using NPS.ID.PublicApi.BinaryApiClient.Connection;
using NPS.Intraday.Edge.PMD.V2;

namespace NPS.ID.PublicApi.BinaryApiClient.Pmd;

/// <summary>
/// Client for the binary Public Market Data (PMD) v2 API.
/// Subscriptions: local view, delivery areas, contracts, ticker, public statistics, H2H and ATC capacity.
/// </summary>
public sealed class PmdClient(BinaryApiSettings settings, SsoClient ssoClient)
    : WebSocketConnection("PMD", ssoClient)
{
    /// <summary>Latest contracts seen on the contract stream (populated after <c>csub</c>).</summary>
    public ContractCache Contracts { get; } = new();

    // -------------------------------------------------------------------------
    // Connection hooks
    // -------------------------------------------------------------------------

    protected override Uri BuildUri(string sessionId, bool isReconnect) =>
        new(string.Format(settings.Pmd.Host, sessionId));

    protected override IMessage BuildTokenRefreshFrame(string token) =>
        new ClientFrame { TokenRefresh = new TokenRefresh { Token = token } };

    protected override Task OnFrameAsync(MemoryStream frame, CancellationToken ct)
    {
        var serverFrame = ServerFrame.Parser.ParseFrom(frame);

        switch (serverFrame.PayloadCase)
        {
            case ServerFrame.PayloadOneofCase.Connected:
                Log("CONNECTED", $"heartbeat_interval_ms={serverFrame.Connected.HeartbeatIntervalMs}");
                break;

            case ServerFrame.PayloadOneofCase.Heartbeat:
                Log("HEARTBEAT", $"received {DateTimeOffset.UtcNow:HH:mm:ss}Z");
                break;

            case ServerFrame.PayloadOneofCase.Error:
                // Log error; if the server subsequently closes the socket, the receive loop will exit.
                LogError("ERROR", serverFrame.Error.Message);
                break;

            case ServerFrame.PayloadOneofCase.LocalView:
                PmdPrinters.PrintLocalView(serverFrame.LocalView);
                break;

            case ServerFrame.PayloadOneofCase.DeliveryArea:
                PmdPrinters.PrintDeliveryArea(serverFrame.DeliveryArea);
                break;

            case ServerFrame.PayloadOneofCase.Contract:
                Contracts.Apply(serverFrame.Contract);
                PmdPrinters.PrintContract(serverFrame.Contract);
                break;

            case ServerFrame.PayloadOneofCase.Ticker:
                PmdPrinters.PrintTicker(serverFrame.Ticker);
                break;

            case ServerFrame.PayloadOneofCase.H2HCapacity:
                PmdPrinters.PrintH2HCapacity(serverFrame.H2HCapacity);
                break;

            case ServerFrame.PayloadOneofCase.PublicStatistic:
                PmdPrinters.PrintPublicStatistic(serverFrame.PublicStatistic);
                break;

            case ServerFrame.PayloadOneofCase.AtcCapacity:
                PmdPrinters.PrintAtcCapacity(serverFrame.AtcCapacity);
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

    // ── Local View ─────────────────────────────────────────────────────────

    /// <summary>Sends a Subscribe frame for local view on the given delivery area IDs.</summary>
    public async Task SubscribeLocalViewAsync(IEnumerable<int> areas, CancellationToken ct)
    {
        var areaList = areas.ToList();
        if (await SendAsync(new ClientFrame
            {
                Subscribe = new Subscribe { LocalView = new LocalViewSubscription { DeliveryAreas = { areaList } } }
            }, ct))
            Log("LV-SUB", $"areas=[{string.Join(", ", areaList)}]");
    }

    /// <summary>Sends an Unsubscribe frame for local view. Pass an empty list to unsubscribe from all areas.</summary>
    public async Task UnsubscribeLocalViewAsync(IEnumerable<int> areas, CancellationToken ct)
    {
        var areaList = areas.ToList();
        if (await SendAsync(new ClientFrame
            {
                Unsubscribe = new Unsubscribe { LocalView = new LocalViewSubscription { DeliveryAreas = { areaList } } }
            }, ct))
            Log("LV-UNSUB", areaList.Count == 0 ? "all areas" : $"areas=[{string.Join(", ", areaList)}]");
    }

    // ── Delivery Area ───────────────────────────────────────────────────────

    public async Task SubscribeDeliveryAreaAsync(CancellationToken ct)
    {
        if (await SendAsync(new ClientFrame { Subscribe = new Subscribe { DeliveryArea = new DeliveryAreaSubscription() } }, ct))
            Log("DA-SUB", "subscribed");
    }

    public async Task UnsubscribeDeliveryAreaAsync(CancellationToken ct)
    {
        if (await SendAsync(new ClientFrame { Unsubscribe = new Unsubscribe { DeliveryArea = new DeliveryAreaSubscription() } }, ct))
            Log("DA-UNSUB", "unsubscribed");
    }

    // ── Contract ────────────────────────────────────────────────────────────

    public async Task SubscribeContractAsync(CancellationToken ct)
    {
        if (await SendAsync(new ClientFrame { Subscribe = new Subscribe { Contract = new ContractSubscription() } }, ct))
            Log("C-SUB", "subscribed");
    }

    public async Task UnsubscribeContractAsync(CancellationToken ct)
    {
        if (await SendAsync(new ClientFrame { Unsubscribe = new Unsubscribe { Contract = new ContractSubscription() } }, ct))
            Log("C-UNSUB", "unsubscribed");
    }

    // ── Ticker ──────────────────────────────────────────────────────────────

    public async Task SubscribeTickerAsync(CancellationToken ct)
    {
        if (await SendAsync(new ClientFrame { Subscribe = new Subscribe { Ticker = new TickerSubscription() } }, ct))
            Log("T-SUB", "subscribed");
    }

    public async Task UnsubscribeTickerAsync(CancellationToken ct)
    {
        if (await SendAsync(new ClientFrame { Unsubscribe = new Unsubscribe { Ticker = new TickerSubscription() } }, ct))
            Log("T-UNSUB", "unsubscribed");
    }

    // ── Public Statistics ───────────────────────────────────────────────────

    public async Task SubscribePublicStatisticAsync(IEnumerable<int> areas, CancellationToken ct)
    {
        var areaList = areas.ToList();
        if (await SendAsync(new ClientFrame
            {
                Subscribe = new Subscribe { PublicStatistic = new PublicStatisticSubscription { DeliveryAreas = { areaList } } }
            }, ct))
            Log("PS-SUB", $"areas=[{string.Join(", ", areaList)}]");
    }

    public async Task UnsubscribePublicStatisticAsync(IEnumerable<int> areas, CancellationToken ct)
    {
        var areaList = areas.ToList();
        if (await SendAsync(new ClientFrame
            {
                Unsubscribe = new Unsubscribe { PublicStatistic = new PublicStatisticSubscription { DeliveryAreas = { areaList } } }
            }, ct))
            Log("PS-UNSUB", areaList.Count == 0 ? "all areas" : $"areas=[{string.Join(", ", areaList)}]");
    }

    // ── H2H Capacity ────────────────────────────────────────────────────────

    public async Task SubscribeH2HCapacityAsync(IEnumerable<CapacityFlow> requests, CancellationToken ct)
    {
        var requestList = requests.ToList();
        if (await SendAsync(new ClientFrame
            {
                Subscribe = new Subscribe { H2HCapacity = new H2HCapacitySubscription { Flows = { requestList } } }
            }, ct))
            Log("H2H-SUB", FormatCapacityRequests(requestList));
    }

    public async Task UnsubscribeH2HCapacityAsync(IEnumerable<CapacityFlow> requests, CancellationToken ct)
    {
        var requestList = requests.ToList();
        if (await SendAsync(new ClientFrame
            {
                Unsubscribe = new Unsubscribe { H2HCapacity = new H2HCapacitySubscription { Flows = { requestList } } }
            }, ct))
            Log("H2H-UNSUB", requestList.Count == 0 ? "all H2H capacity areas" : FormatCapacityRequests(requestList));
    }

    // ── ATC Capacity ────────────────────────────────────────────────────────

    public async Task SubscribeAtcCapacityAsync(IEnumerable<CapacityFlow> requests, CancellationToken ct)
    {
        var requestList = requests.ToList();
        if (await SendAsync(new ClientFrame
            {
                Subscribe = new Subscribe { AtcCapacity = new AtcCapacitiesSubscription { Flows = { requestList } } }
            }, ct))
            Log("ATC-SUB", FormatCapacityRequests(requestList));
    }

    public async Task UnsubscribeAtcCapacityAsync(IEnumerable<CapacityFlow> requests, CancellationToken ct)
    {
        var requestList = requests.ToList();
        if (await SendAsync(new ClientFrame
            {
                Unsubscribe = new Unsubscribe { AtcCapacity = new AtcCapacitiesSubscription { Flows = { requestList } } }
            }, ct))
            Log("ATC-UNSUB", requestList.Count == 0 ? "all ATC capacity areas" : FormatCapacityRequests(requestList));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string FormatCapacityRequests(List<CapacityFlow> requests) =>
        string.Join("; ", requests.Select(r =>
            $"{r.DeliveryArea}->[{string.Join(",", r.ConnectedDeliveryAreas)}]"));
}
