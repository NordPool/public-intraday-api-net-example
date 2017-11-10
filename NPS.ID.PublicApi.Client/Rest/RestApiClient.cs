/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool’s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using System;
using Newtonsoft.Json;
using NPS.ID.PublicApi.Client.Security.Exceptions;
using NPS.ID.PublicApi.Client.Utilities;
using Nordpool.ID.PublicApi.v1;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Client.Rest
{
    public class RestApiClient
    {
        private readonly string _host;
        private readonly string _protocol;
        private readonly string _token;
        private readonly string _apiversion;

        private const string datetTimeFormat = "yyyy-MM-ddTHH\\:mm\\:ss.FFFZ";

        public RestApiClient(string host, string protocol, string token, string apiversion)
        {
            _host = host;
            _protocol = protocol;
            _token = token;
            _apiversion = apiversion;
        }
        
        /// <summary>
        /// Get historical private trades
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<List<PrivateTradeRow>> GetPrivateTrades(DateTimeOffset start, DateTimeOffset end)
        {
            var restConnector = new GenericRestConnector(_token);
            

            try
            {

                var startZuluTime = start.ToString(datetTimeFormat);
                var endZuluTime = end.ToString(datetTimeFormat);
                var response = await restConnector.Get<List<PrivateTradeRow>>(ConstructRestProxyUri($"privateTrade?startZuluTime={startZuluTime}&endZuluTime={endZuluTime}"));

                return response;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get private trades", e);
            }
        }

        /// <summary>
        /// Get historical order executions
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<OrderExecutionReport> GetOrderExecutions(DateTimeOffset start, DateTimeOffset end)
        {
            var restConnector = new GenericRestConnector(_token);
            try
            {
                var startZuluTime = start.ToString(datetTimeFormat);
                var endZuluTime = end.ToString(datetTimeFormat);
                var response = await restConnector.Get<OrderExecutionReport>(ConstructRestProxyUri($"orderExecutionReport?startZuluTime={startZuluTime}&endZuluTime={endZuluTime}"));

                return response;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get private trades", e);
            }
        }

        /// <summary>
        /// Get historical public trades
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<List<PublicTradeRow>> GetPublicTrades(DateTimeOffset start, DateTimeOffset end)
        {
            var restConnector = new GenericRestConnector(_token);
            try
            {
                var startZuluTime = start.ToString(datetTimeFormat);
                var endZuluTime = end.ToString(datetTimeFormat);
                var response = await restConnector.Get<List<PublicTradeRow>>(ConstructRestProxyUri($"publicTrade?startZuluTime={startZuluTime}&endZuluTime={endZuluTime}"));

                return response;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get private trades", e);
            }
        }

        private string ConstructRestProxyUri(string operation)
        {
            return $"{_protocol}://{_host}/api/{_apiversion}/{operation}";
        }

    }
}