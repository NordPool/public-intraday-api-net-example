using NPS.ID.PublicApi.DotNet.Client.Connection.Messages;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Events;

public class MessageReceivedEventArgs : EventArgs
{
    public ReceivedMessage Message { get; init; }

    public DateTimeOffset Timestamp { get; init; }
}