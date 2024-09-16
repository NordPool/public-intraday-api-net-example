using NPS.ID.PublicApi.DotNet.Client.Connection.Enums;
using NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions.Helpers;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions.Requests;

public class SubscribeRequest
{
    public string SubscriptionId { get; }
    public string Destination { get; }
    public string Type { get; }

    private SubscribeRequest(string subscriptionId, string type, string destination)
    {
        SubscriptionId = subscriptionId;
        Destination = destination;
        Type = type;
    }

    public static SubscribeRequest DeliveryAreas(string subscriptionId, string user, string version)
    {
        return new SubscribeRequest(subscriptionId, "delivery_areas", DestinationHelper.ComposeDestination(user, version, PublishingMode.Streaming, "deliveryAreas"));
    }
    
    public static SubscribeRequest Configuration(string subscriptionId, string user, string version)
    {
        return new SubscribeRequest(subscriptionId, "configuration", DestinationHelper.ComposeDestination(user, version, "configuration"));
    }
    
    public static SubscribeRequest OrderExecutionReports(string subscriptionId, string user, string version, PublishingMode mode)
    {
        return new SubscribeRequest(subscriptionId, "order_execution_report", DestinationHelper.ComposeDestination(user, version, mode, "orderExecutionReport"));
    }
    
    public static SubscribeRequest Contracts(string subscriptionId, string user, string version, PublishingMode mode)
    {
        return new SubscribeRequest(subscriptionId, "contracts", DestinationHelper.ComposeDestination(user, version, mode, "contracts"));
    }
    
    public static SubscribeRequest LocalView(string subscriptionId, string user, string version, PublishingMode mode, int deliveryAreaId)
    {
        return new SubscribeRequest(subscriptionId, "localview", DestinationHelper.ComposeDestination(user, version, mode, $"localview/{deliveryAreaId}"));
    }

    public static SubscribeRequest PrivateTrades(string subscriptionId, string user, string version, PublishingMode mode)
    {
        return new SubscribeRequest(subscriptionId, "private_trade", DestinationHelper.ComposeDestination(user, version, mode, "privateTrade"));
    }

    public static SubscribeRequest Ticker(string subscriptionId, string user, string version, PublishingMode mode)
    {
        return new SubscribeRequest(subscriptionId, "ticker", DestinationHelper.ComposeDestination(user, version, mode, "ticker"));
    }

    public static SubscribeRequest MyTicker(string subscriptionId, string user, string version, PublishingMode mode)
    {
        return new SubscribeRequest(subscriptionId, "my_ticker", DestinationHelper.ComposeDestination(user, version, mode, "myTicker"));
    }
    
    public static SubscribeRequest PublicStatistics(string subscriptionId, string user, string version, PublishingMode mode, int deliveryAreaId)
    {
        return new SubscribeRequest(subscriptionId, "public_statistics", DestinationHelper.ComposeDestination(user, version, mode, $"publicStatistics/{deliveryAreaId}"));
    }

    public static SubscribeRequest ThrottlingLimits(string subscriptionId, string user, string version, PublishingMode mode)
    {
        return new SubscribeRequest(subscriptionId, "throttling_limits", DestinationHelper.ComposeDestination(user, version, mode, "throttlingLimits"));
    }
    
    public static SubscribeRequest Capacities(string subscriptionId, string user, string version, PublishingMode mode, int deliveryAreaId, IEnumerable<int> additionalDeliveryAreas)
    {
        var additionalAreasPart = additionalDeliveryAreas.Any() ? $"/{string.Join("-", additionalDeliveryAreas)}" : string.Empty;
        return new SubscribeRequest(subscriptionId, "capacities", DestinationHelper.ComposeDestination(user, version, mode, $"capacities/{deliveryAreaId}{additionalAreasPart}"));
    }

    public static SubscribeRequest Heartbeat(string subscriptionId, string user, string version)
    {
        return new SubscribeRequest(subscriptionId, "heartbeat", DestinationHelper.ComposeDestination(user, version, "heartbeatping"));
    }

    public static SubscribeRequest MarketInfo(string subscriptionId, string user, string version, PublishingMode mode, int deliveryAreaId)
    {
        return new SubscribeRequest(subscriptionId, "market_info", DestinationHelper.ComposeDestination(user, version, mode, $"marketinfo/{deliveryAreaId}"));
    }
}