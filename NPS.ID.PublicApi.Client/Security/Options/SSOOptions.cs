namespace NPS.ID.PublicApi.Client.Security.Options;

public record SsoOptions
{
    public const string SectionName = "Sso";
    public Uri Uri { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}
