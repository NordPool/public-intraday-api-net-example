using System.Text;
using System.Text.Json;
using NPS.ID.PublicApi.Client.Security.Exceptions;
using NPS.ID.PublicApi.Client.Security.Responses;

namespace NPS.ID.PublicApi.Client.Security;

public class SsoService : ISsoService
{
    private readonly string _grantType;
    private readonly string _scope;

    private readonly HttpClient _httpClient;

    public SsoService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient("SsoClient");
        _grantType = "password";
        _scope = "global";
    }

    public async Task<string> GetAuthTokenAsync(string userName, string password, CancellationToken cancellationToken)
    {
        var payload = ConstructPayloadForTokenRequest(userName, password);

        try
        {
            const string mediaType = "application/x-www-form-urlencoded";
            var response = await _httpClient.PostAsync("", new StringContent(payload, Encoding.UTF8, mediaType), cancellationToken);
            response.EnsureSuccessStatusCode();
            var tokenResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<AccessTokenResponse>(tokenResponse).Token;
        }
        catch (Exception e)
        {
            throw new TokenRequestFailedException("Failed to retrieve auth token! Check username and password!", e);
        }
    }

    private string ConstructPayloadForTokenRequest(string userName, string password)
    {
        return $"grant_type={_grantType}&scope={_scope}&username={userName}&password={System.Net.WebUtility.UrlEncode(password)}";
    }
}
