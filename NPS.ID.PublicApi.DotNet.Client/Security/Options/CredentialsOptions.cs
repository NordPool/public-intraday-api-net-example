namespace NPS.ID.PublicApi.DotNet.Client.Security.Options;

public record CredentialsOptions
{
    public const string SectionName = "Credentials";
    public string UserName { get; set; }
    public string Password { get; set; }
}