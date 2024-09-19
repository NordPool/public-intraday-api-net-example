using System.Text;

namespace NPS.ID.PublicApi.Client.Connection.Messages;

public static class WebSocketMessages
{
    private static readonly Encoding DefaultEncoding = Encoding.UTF8;

    public static readonly byte[] NewLine = DefaultEncoding.GetBytes("\\n");
    public static readonly byte[] SockJsStart = DefaultEncoding.GetBytes("o");
    public static readonly byte[] ServerHeartBeat = DefaultEncoding.GetBytes("h");
    public static readonly byte[] ClientHeartBeat = DefaultEncoding.GetBytes("[\"\\n\"]");
    public static readonly byte[] DisconnectCode = DefaultEncoding.GetBytes("[\"1000\"]");
    public static readonly byte[] ConnectedPrefix = DefaultEncoding.GetBytes("a[\"CONNECTED");
    public static readonly byte[] MessagePrefix = DefaultEncoding.GetBytes("a[\"MESSAGE");
    public static readonly byte[] Logout = DefaultEncoding.GetBytes("[\"SEND\\ndestination:/v1/command\\ncontent-length:17\\n\\n{\\\"type\\\":\\\"LOGOUT\\\"}\\u0000\"]");
}