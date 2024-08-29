namespace NPS.ID.PublicApi.DotNet.Client.Connection.Options;

public record EndpointsOptions
{
    public const string SectionName = "Endpoints";
    
    public WebSocketOptions Middleware { get; set; }
    public WebSocketOptions Edge { get; set; }
}