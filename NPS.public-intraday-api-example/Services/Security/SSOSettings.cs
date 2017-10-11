namespace NPS.public_intraday_api_example.Services.Security
{
    public class SSOSettings
    {
        public string Host { get; set; }

        public string TokenUri { get; set; }

        public string Protocol { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Scope { get; set; }
    }
}