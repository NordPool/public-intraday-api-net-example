using Newtonsoft.Json;

namespace NPS.ID.PublicApi.DotNet.Client.Security.Responses;

public class AccessTokenResponse
{
    [JsonProperty("access_token")]
    public string Token { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty("token_type")]
    public string TokenType { get; set; }
}