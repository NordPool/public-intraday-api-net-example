using NPS.ID.PublicApi.DotNet.Client.Connection.Enums;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions.Requests;

public class SubscribeRequestBuilder
{
    private static int _subCounter;
    
    private readonly string _user;
    private readonly string _version;

    private SubscribeRequestBuilder(string user, string version)
    {
        _user = user;
        _version = version;
    }

    public static SubscribeRequestBuilder CreateBuilder(string user, string version)
    {
        return new SubscribeRequestBuilder(user, version);
    }

    public SubscribeRequest CreateDeliveryAreas()
    {
        return SubscribeRequest.DeliveryAreas(GetSubId(), _user, _version);
    }
    
    public SubscribeRequest CreateConfiguration()
    {
        return SubscribeRequest.Configuration(GetSubId(), _user, _version);
    }
    
    public SubscribeRequest CreateOrderExecutionReport(PublishingMode publishingMode)
    {
        return SubscribeRequest.OrderExecutionReports(GetSubId(), _user, _version, publishingMode);
    }
    
    public SubscribeRequest CreateContracts(PublishingMode publishingMode)
    {
        return SubscribeRequest.Contracts(GetSubId(), _user, _version, publishingMode);
    }
    
    public SubscribeRequest CreateLocalViews(PublishingMode publishingMode, int deliveryAreaId)
    {
        return SubscribeRequest.LocalView(GetSubId(), _user, _version, publishingMode, deliveryAreaId);
    }

    public SubscribeRequest CreatePrivateTrades(PublishingMode publishingMode)
    {
        return SubscribeRequest.PrivateTrades(GetSubId(), _user, _version, publishingMode);
    }
    
    public SubscribeRequest CreateTicker(PublishingMode publishingMode)
    {
        return SubscribeRequest.Ticker(GetSubId(), _user, _version, publishingMode);
    }

    public SubscribeRequest CreateMyTicker(PublishingMode publishingMode)
    {
        return SubscribeRequest.MyTicker(GetSubId(), _user, _version, publishingMode);
    }
    
    public SubscribeRequest CreatePublicStatistics(PublishingMode publishingMode, int deliveryAreaId)
    {
        return SubscribeRequest.PublicStatistics(GetSubId(), _user, _version, publishingMode, deliveryAreaId);
    }

    public SubscribeRequest CreateThrottlingLimits(PublishingMode publishingMode)
    {
        return SubscribeRequest.ThrottlingLimits(GetSubId(), _user, _version, publishingMode);
    }
    
    public SubscribeRequest CreateCapacities(PublishingMode publishingMode, int deliveryAreaId)
    {
        return CreateCapacities(publishingMode, deliveryAreaId, Array.Empty<int>());
    }

    public SubscribeRequest CreateCapacities(PublishingMode publishingMode, int deliveryAreaId, IEnumerable<int> additionalDeliveryAreas)
    {
        return SubscribeRequest.Capacities(GetSubId(), _user, _version, publishingMode, deliveryAreaId, additionalDeliveryAreas);
    }

    public SubscribeRequest CreateHeartBeat()
    {
        return SubscribeRequest.Heartbeat(GetSubId(), _user, _version);
    }

    public SubscribeRequest CreateMarketInfo(PublishingMode publishingMode, int deliveryAreaId)
    {
        return SubscribeRequest.MarketInfo(GetSubId(), _user, _version, publishingMode, deliveryAreaId);
    }

    private string GetSubId()
    {
        return $"sub-{Interlocked.Increment(ref _subCounter)}";
    }
}