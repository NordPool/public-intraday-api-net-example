namespace NPS.ID.PublicApi.BinaryApiClient;

public class BinaryApiSettings
{
    /// <summary>Public market data (PMD) endpoint settings.</summary>
    public PmdSettings Pmd { get; set; } = new();

    /// <summary>Trading endpoint settings.</summary>
    public TradingSettings Trading { get; set; } = new();

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

public class PmdSettings
{
    /// <summary>WebSocket URL template with a {0} placeholder for the session id, e.g. wss://host/api/v2/{0}</summary>
    public string Host { get; set; } = "";
}

public class TradingSettings
{
    /// <summary>WebSocket URL template with a {0} placeholder for the session id, e.g. wss://host/api/v2/{0}</summary>
    public string Host { get; set; } = "";

    /// <summary>
    /// Appends <c>?autoHibe=true</c> to the connection URL: orders are automatically deactivated
    /// if client heartbeats stop for 10 s or the connection is lost without a graceful close.
    /// When enabled the client MUST send a Heartbeat frame every second — this client does so automatically.
    /// </summary>
    public bool AutoHibe { get; set; }

    /// <summary>
    /// Appends <c>abortExisting=true</c> on reconnect attempts, asking the server to abort a stale
    /// previous session instead of rejecting the new connection.
    /// </summary>
    public bool AbortExisting { get; set; }
}
