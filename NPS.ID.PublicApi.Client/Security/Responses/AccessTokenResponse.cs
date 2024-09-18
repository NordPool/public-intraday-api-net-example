using System.Text.Json.Serialization;

namespace NPS.ID.PublicApi.Client.Security.Responses;

public class AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string Token { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }
}