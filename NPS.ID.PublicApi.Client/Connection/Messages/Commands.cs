namespace NPS.ID.PublicApi.Client.Connection.Messages;

public static class Commands
{
    public static class Client
    {
        public const string Connect = "CONNECT";
        public const string Disconnect = "DISCONNECT";
        public const string Send = "SEND";
        public const string Subscribe = "SUBSCRIBE";
        public const string Unsubscribe = "UNSUBSCRIBE";
        public const string KeepAlive = "KEEPALIVE";
    }

    public static class Server
    {
        public const string Connected = "CONNECTED";
        public const string Message = "MESSAGE";
        public const string Error = "ERROR";
        public const string Receipt = "RECEIPT"; 
    }
}