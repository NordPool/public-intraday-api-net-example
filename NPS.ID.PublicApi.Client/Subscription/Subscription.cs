using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Client.Subscription
{
    public class Subscription
    {
        public Subscription()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        public Topic Topic { get; set; }
        public string Version { get; set; }
        public SubscriptionType SubscriptionType { get; set; }

        public int Area { get; set; }

        public bool IsGZipped { get; set; }

        public string Id { get; }

        public static Builder Builder()
        {
            var builder = new SubscriptionBuilder();
            return builder.Begin();
        } 
    }

    public abstract class Builder
    {
        protected Builder()
        {
        }

        public abstract Builder Begin();
        public abstract Builder WithVersion(string version);
        public abstract Builder WithArea(int area);

        public abstract Builder WithIsGzipped(bool isGzipped);

        public abstract Builder WithTopic(Topic topic);

        public abstract Builder WithSubscriptionType(SubscriptionType subscriptionType);

        public abstract Subscription Build();
    }

    public class SubscriptionBuilder : Builder
    {
        private Subscription _subscription;

        public override Builder Begin()
        {
            _subscription = new Subscription();
            return this;
        }

        public override Builder WithVersion(string version)
        {
            _subscription.Version = version;
            return this;
        }

        public override Builder WithArea(int area)
        {
            _subscription.Area = area;
            return this;
        }

        public override Builder WithIsGzipped(bool isGzipped)
        {
            _subscription.IsGZipped = isGzipped;
            return this;
        }

        public override Builder WithTopic(Topic topic)
        {
            _subscription.Topic = topic;
            return this;
        }

        public override Builder WithSubscriptionType(SubscriptionType subscriptionType)
        {
            _subscription.SubscriptionType = subscriptionType;
            return this;
        }

        public override Subscription Build()
        {
            return _subscription;
        }
    }

    public enum Topic
    {
        LocalView,
        Capacities,
        DeliveryAreas,
        Configuration,
        OrderExecutionReport,
        PrivateTrade,
        Ticker,
        PublicStatistics,
        Contracts,
        HeartbeatPing
    }

    public enum SubscriptionType
    {
        Streaming,
        Conflated,
        Empty
    }
}