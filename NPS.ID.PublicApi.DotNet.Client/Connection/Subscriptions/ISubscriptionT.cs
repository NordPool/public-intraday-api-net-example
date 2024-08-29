using System.Threading.Channels;
using NPS.ID.PublicApi.DotNet.Client.Connection.Messages;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions;

public interface ISubscription<TValue> : ISubscription
{
    public ChannelReader<ReceivedMessage<IReadOnlyCollection<TValue>>> OutputChannel { get; }
}