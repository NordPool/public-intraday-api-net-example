using NPS.ID.PublicApi.DotNet.Client.Connection.Enums;

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

    public static SubscribeRequest MarketInfo(string subscriptionId, string user, string version, PublishingMode mode, int deliveryAreaId)
    {
        return new SubscribeRequest(subscriptionId, "market_info", ComposeDestination(user, version, mode, $"marketinfo/{deliveryAreaId}"));
    }

    public static SubscribeRequest LocalView(string subscriptionId, string user, string version, PublishingMode mode, int deliveryAreaId)
    {
        return new SubscribeRequest(subscriptionId, "localview", ComposeDestination(user, version, mode, $"localview/{deliveryAreaId}"));
    }

    public static SubscribeRequest Capacities(string subscriptionId, string user, string version, PublishingMode mode, int deliveryAreaId, IEnumerable<int> additionalDeliveryAreas)
    {
        var additionalAreasPart = additionalDeliveryAreas.Any() ? $"/{string.Join("-", additionalDeliveryAreas)}" : string.Empty;
        return new SubscribeRequest(subscriptionId, "capacities", ComposeDestination(user, version, mode, $"capacities/{deliveryAreaId}{additionalAreasPart}"));
    }

    public static SubscribeRequest Contracts(string subscriptionId, string user, string version, PublishingMode mode)
    {
        return new SubscribeRequest(subscriptionId, "contracts", ComposeDestination(user, version, mode, "contracts"));
    }

    public static SubscribeRequest Ticker(string subscriptionId, string user, string version, PublishingMode mode)
    {
        return new SubscribeRequest(subscriptionId, "ticker", ComposeDestination(user, version, mode, "ticker"));
    }

    public static SubscribeRequest MyTicker(string subscriptionId, string user, string version, PublishingMode mode)
    {
        return new SubscribeRequest(subscriptionId, "my_ticker", ComposeDestination(user, version, mode, "myTicker"));
    }

    public static SubscribeRequest Heartbeat(string subscriptionId, string user, string version)
    {
        return new SubscribeRequest(subscriptionId, "heartbeat", ComposeDestination(user, version, "heartbeatping"));
    }

    public static SubscribeRequest Configuration(string subscriptionId, string user, string version)
    {
        return new SubscribeRequest(subscriptionId, "configuration", ComposeDestination(user, version, "configuration"));
    }

    public static SubscribeRequest OrderExecutionReports(string subscriptionId, string user, string version, PublishingMode mode)
    {
        return new SubscribeRequest( subscriptionId, "order_execution_report", ComposeDestination(user, version, mode, "orderExecutionReport"));
    }
    
    public static SubscribeRequest DeliveryAreas(string subscriptionId, string user, string version)
    {
        return new SubscribeRequest( subscriptionId, "delivery_areas", ComposeDestination(user, version, PublishingMode.Streaming, "deliveryAreas"));
    }

    private static string ComposeDestination(string user, string version, PublishingMode mode, string topic)
    {
        return ComposeDestination(user, version, $"{mode.ToString().ToLower()}/{topic}");
    }
    
    private static string ComposeDestination(string user, string version, string topic)
    {
        return $"/user/{user}/{version}/{topic}";
    }
}