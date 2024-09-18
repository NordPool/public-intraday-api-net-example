namespace NPS.ID.PublicApi.Client.Connection.Options;

public record EndpointsOptions
{
    public const string SectionName = "Endpoints";
    public WebSocketOptions Trading { get; set; }
    public WebSocketOptions MarketData { get; set; }
}