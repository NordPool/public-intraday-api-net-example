namespace NPS.ID.PublicApi.DotNet.Client.Connection.Options;

public record WebSocketOptions
{
    public bool UseSsl { get; set; } = true;
    public int Port { get; set; } = 80;
    public int SslPort { get; set; } = 443;
    public string Host { get; set; }
    public string Uri { get; set; } = "/user";
    public int HeartbeatOutgoingInterval { get; set; } = 1000;
    
    public int UsedPort => UseSsl ? SslPort : Port;
}