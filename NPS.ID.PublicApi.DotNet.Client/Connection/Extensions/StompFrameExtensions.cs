using System.Text;
using Newtonsoft.Json;
using NPS.ID.PublicApi.DotNet.Client.Connection.Enums;
using NPS.ID.PublicApi.DotNet.Client.Connection.Messages;
using Stomp.Net.Stomp.Protocol;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Extensions;

public static class StompFrameExtensions
{
    public static bool IsSnapshot(this StompFrame frame)
    {
        return frame.Properties.TryGetValue(Headers.Server.IsSnapshot, out var isSnapshotString) && string.Equals(isSnapshotString, "true");
    }
    
    public static PublishingMode GetPublishingMode(this StompFrame frame)
    {
        return frame.Properties.TryGetValue(Headers.Destination, out var dest) && dest.Contains("/streaming/")
            ? PublishingMode.Streaming
            : PublishingMode.Conflated;
    }
    
    public static DateTimeOffset? GetSentAtTimestamp(this StompFrame frame)
    {
        return frame.Properties.TryGetValue(Headers.Server.SentAt, out var sentAtHeaderValue) && long.TryParse(sentAtHeaderValue, out var sentAtMs)
            ? DateTimeOffset.FromUnixTimeMilliseconds(sentAtMs)
            : null;
    }
    
    public static Task SendStompFrameAsync(this WebSocketConnector connector, StompFrame frame, CancellationToken cancellationToken)
    {
        return connector.SendAsync(frame.ConvertToMessageBytes(), cancellationToken);
    }

    public static byte[] ConvertToMessageBytes(this StompFrame frame)
    {
        var messageText = frame.ToMessageText();
        var serializedJsonArray = JsonConvert.SerializeObject(new[] { messageText });
        var messageBytes = Encoding.UTF8.GetBytes(serializedJsonArray);
        return messageBytes;
    }

    private static string ToMessageText(this StompFrame frame)
    {
        var bytes = FrameToBytes(frame);
        var stringOfBytes = Encoding.UTF8.GetString(bytes);

        return stringOfBytes;
    }

    private static byte[] FrameToBytes(StompFrame frame)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms, Encoding.UTF8);

        frame.ToStream(bw);

        return ms.ToArray();
    }
}