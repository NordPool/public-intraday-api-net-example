using Microsoft.IO;
using NPS.ID.PublicApi.Client.Connection.Enums;

namespace NPS.ID.PublicApi.Client.Connection.Messages;

public record ReceivedMessage<T>(T Data, DateTimeOffset Timestamp, bool IsSnapshot, PublishingMode PublishingMode)
{
    public static implicit operator T(ReceivedMessage<T> msg)
    {
        return msg.Data;
    }
}

public sealed class ReceivedMessage : IDisposable
{
    private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();

    private readonly RecyclableMemoryStream _messageStream;

    public ReceivedMessage()
    {
        _messageStream = MemoryStreamManager.GetStream(tag: "received-message-stream");
    }

    public bool IsSockJsStart() => Is(WebSocketMessages.SockJsStart);
    public bool IsHeartBeat() => Is(WebSocketMessages.ServerHeartBeat);
    public bool IsDisconnectCode() => Is(WebSocketMessages.DisconnectCode);
    public bool IsConnectedCommand() => Is(WebSocketMessages.ConnectedPrefix, compareLength: false);
    public bool IsMessageCommand() => Is(WebSocketMessages.MessagePrefix, compareLength: false);
    public bool IsError() => Is(WebSocketMessages.ErrorPrefix, compareLength: false);

    private bool Is(byte[] other, bool compareLength = true)
    {
        if (compareLength && other.Length != _messageStream.Length)
        {
            return false;
        }

        return _messageStream.GetSpan()[..other.Length].SequenceEqual(other);
    }

    public void Append(byte[] bytes, int offset, int length, bool isLast)
    {
        _messageStream.Write(bytes, offset, length);
        if (isLast)
        {
            _messageStream.Position = 0;
        }
    }

    public Stream GetStream() => _messageStream;
    
    public void Dispose()
    {
        _messageStream.Dispose();
    }
}