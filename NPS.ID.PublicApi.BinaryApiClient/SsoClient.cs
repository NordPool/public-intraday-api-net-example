using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace NPS.ID.PublicApi.BinaryApiClient;

public class SsoClient
{
    private readonly BinaryApiSettings _settings;
    private readonly HttpClient _http;

    public SsoClient(BinaryApiSettings settings)
    {
        _settings = settings;
        _http = new HttpClient();
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{settings.ClientId}:{settings.ClientSecret}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<string> GetToken()
    {
        var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("scope", "global"),
            new("username", _settings.Username),
            new("password", _settings.Password)
        });

        var response = await _http.PostAsync(_settings.SsoUrl, content);

        response.EnsureSuccessStatusCode();

        var ssoResponse = await response.Content.ReadFromJsonAsync<SsoResponse>();
        return ssoResponse!.access_token;
    }

    private class SsoResponse
    {
        public string access_token { get; set; } = "";
    }
}