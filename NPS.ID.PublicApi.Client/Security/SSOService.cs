/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool’s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using System;
using Newtonsoft.Json;
using NPS.ID.PublicApi.Client.Security.Exceptions;
using NPS.ID.PublicApi.Client.Utilities;

namespace NPS.ID.PublicApi.Client.Security
{
    public class SSOService
    {
        private readonly string _host;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tokenUri;
        private readonly string _protocol;
        private readonly string _grantType;
        private readonly string _scope;

        public SSOService(string host, string clientId, string clientSecret, string tokenUri = "/connect/token",
            string protocol = "https", string grantType = "password", string scope = "intraday_api")
        {
            _host = host;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tokenUri = tokenUri;
            _protocol = protocol;
            _grantType = grantType;
            _scope = scope;
        }

        public SSOService(SSOSettings ssoSettings)
        {
            _host = ssoSettings.Host;
            _clientId = ssoSettings.ClientId;
            _clientSecret = ssoSettings.ClientSecret;
            _tokenUri = ssoSettings.TokenUri;
            _protocol = ssoSettings.Protocol;
            _grantType = "password";
            _scope = "intraday_api";
        }

        /// <summary>
        /// Gets the auth token with the parameters given in the constructor
        /// </summary>
        /// <returns>the auth token as string</returns>
        public async System.Threading.Tasks.Task<string> GetAuthToken(string userName, string password)
        {
            var restConnector = new GenericRestConnector(new BasicCredentials()
            {
                Password = _clientSecret,
                UserName = _clientId
            });

            var payload = ConstructPayloadForTokenRequest(userName, password);

            try
            {
                var tokenResponse = await restConnector.PostRawString<AccessTokenResponse>(ConstructSSOServiceUri(), payload)
                    ;

                return tokenResponse.Token;
            }
            catch (Exception e)
            {
                throw new TokenRequestFailedException("Failed to retrieve auth token! Check username and password!", e);
            }
        }


        /// <summary>
        /// Gets the auth token with the parameters given in the constructor except overrides clientId and clientSecret with given params.
        /// </summary>
        /// <param name="clientId">clientId for retrieving the auth token</param>
        /// <param name="clientSecret">clientSecret for retrieving the auth token</param>
        /// <returns>the auth token as string</returns>
        public string GetAuthToken(string userName, string password, string clientId, string clientSecret)
        {
            var restConnector = new GenericRestConnector(new BasicCredentials()
            {
                Password = clientSecret,
                UserName = clientId
            });

            var payload = ConstructPayloadForTokenRequest(userName, password);

            try
            {
                var tokenResponse = restConnector.PostRawString<AccessTokenResponse>(ConstructSSOServiceUri(), payload)
                    .Result;

                return tokenResponse.Token;
            }
            catch (Exception e)
            {
                throw new TokenRequestFailedException("Failed to retrieve auth token! Check username and password!", e);
            }
        }

        private string ConstructSSOServiceUri()
        {
            return $"{_protocol}://{_host}{_tokenUri}";
        }

        private string ConstructPayloadForTokenRequest(string userName, string password)
        {
            return $"grant_type={_grantType}&scope={_scope}&username={userName}&password={System.Net.WebUtility.UrlEncode(password)}";
        }
    }

    public class AccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string Token { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}