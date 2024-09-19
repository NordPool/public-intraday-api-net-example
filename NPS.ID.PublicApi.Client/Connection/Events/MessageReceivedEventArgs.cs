using NPS.ID.PublicApi.Client.Connection.Messages;

namespace NPS.ID.PublicApi.Client.Connection.Events;

public class MessageReceivedEventArgs : EventArgs
{
    public ReceivedMessage Message { get; init; }

    public DateTimeOffset Timestamp { get; init; }
}