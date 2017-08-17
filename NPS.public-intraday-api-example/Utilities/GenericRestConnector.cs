using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace NPS.public_intraday_api_example.Utilities
{
    public class GenericRestConnector
    {
        private readonly BasicCredentials _credentials;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public GenericRestConnector()
        {
        }

        public GenericRestConnector(BasicCredentials credentials)
        {
            _credentials = credentials;
        }

        public async Task<T> Get<T>(string uri)
        {
            using (var client = GetHttpClient())
            {
                return await client.GetAsync(uri).ContinueWith(response =>
                {
                    response.Result.EnsureSuccessStatusCode();
                    return response.Result.Content.ReadAsStringAsync().ContinueWith(responseString =>
                    {
                        return JsonConvert.DeserializeObject<T>(responseString.Result);
                    }).Result;
                });
            }
        }

        public async Task<string> GetRawPayload(string uri)
        {
            using (var client = GetHttpClient())
            {
                return await client.GetAsync(uri).ContinueWith(response =>
                {
                    response.Result.EnsureSuccessStatusCode();
                    return response.Result.Content.ReadAsStringAsync();
                }).Result;
            }
        }

        public async Task<string> PostRawString(string uri, string rawPayload,
            string mediaType = "application/x-www-form-urlencoded")
        {
            using (var client = GetHttpClient())
            {
                return await client.PostAsync(uri, new StringContent(rawPayload, Encoding.UTF8, mediaType))
                    .ContinueWith(response =>
                    {
                        response.Result.EnsureSuccessStatusCode();
                        return response.Result.Content.ReadAsStringAsync();
                    }).Result;
            }
        }


        public async Task<T> PostRawString<T>(string uri, string rawPayload,
            string mediaType = "application/x-www-form-urlencoded")
        {
            using (var client = GetHttpClient())
            {
                return await client.PostAsync(uri, new StringContent(rawPayload, Encoding.UTF8, mediaType))
                    .ContinueWith(response =>
                    {
                        // Will throw if user and password is not set in App.config. 
                        response.Result.EnsureSuccessStatusCode();

                        return response.Result.Content.ReadAsStringAsync().ContinueWith(responseString =>
                        {
                            return JsonConvert.DeserializeObject<T>(responseString.Result);
                        }).Result;
                    });
            }
        }


        private HttpClient GetHttpClient()
        {
            if (_credentials.Equals(default(BasicCredentials)))
                return HttpClientFactory.Create();

            return HttpClientFactory.CreateWithBasicAuth(_credentials.Password, _credentials.UserName);
        }
    }

    public struct BasicCredentials
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}