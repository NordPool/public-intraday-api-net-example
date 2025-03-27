namespace NPS.ID.PublicApi.Client.Connection.Options;

public record WebSocketOptions
{
    public int SslPort { get; set; } = 443;
    public string Host { get; set; }
    public string Uri { get; set; } = "/user";
    public int HeartbeatOutgoingInterval { get; set; } = 1000;
    public bool EnablePermessageDeflate { get; set; } = false;
}