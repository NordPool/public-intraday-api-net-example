namespace NPS.ID.PublicApi.Client.Connection.Messages;

public static class Headers
{
    public const string Heartbeat = "heart-beat";
    public const string Destination = "destination";
    public const string ContentType = "content-type";
    public const string ContentLength = "content-length";

    public static class Client
    {
        public const string AuthorizationToken = "X-AUTH-TOKEN";
        public const string AcceptVersion = "accept-version";
        public const string SubscriptionId = "id";
    }
    
    public static class Server
    {
        public const string Version = "version";  
        public const string Message = "message";
        public const string IsSnapshot = "x-nps-snapshot";
        public const string Subscription = "subscription";
        public const string SequenceNumber = "x-nps-sequenceNo";
        public const string SentAt = "x-nps-sent-at";
    }
}