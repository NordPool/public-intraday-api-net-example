namespace NPS.ID.PublicApi.BinaryApiClient;

public class BinaryApiSettings
{
    /// <summary>WebSocket base URL, e.g. wss://host/api/v2</summary>
    public string Host { get; set; } = "";

    /// <summary>SSO token endpoint URL.</summary>
    public string SsoUrl { get; set; } = "";

    /// <summary>Username for SSO authentication.</summary>
    public string Username { get; set; } = "";

    /// <summary>Password for SSO authentication.</summary>
    public string Password { get; set; } = "";

    /// <summary>OAuth client ID for Basic auth header.</summary>
    public string ClientId { get; set; } = "";

    /// <summary>OAuth client secret for Basic auth header.</summary>
    public string ClientSecret { get; set; } = "";
}
