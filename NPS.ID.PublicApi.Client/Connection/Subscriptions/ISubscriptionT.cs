using System.Threading.Channels;
using NPS.ID.PublicApi.Client.Connection.Messages;

namespace NPS.ID.PublicApi.Client.Connection.Subscriptions;

public interface ISubscription<TValue> : ISubscription
{
    public ChannelReader<ReceivedMessage<IReadOnlyCollection<TValue>>> OutputChannel { get; }
}