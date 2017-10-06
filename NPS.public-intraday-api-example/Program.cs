using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Threading;
using log4net;
using NPS.public_intraday_api_example.Services.Connection;
using NPS.public_intraday_api_example.Services.Security;
using NPS.public_intraday_api_example.Services.Subscription;
using NPS.public_intraday_api_example.Services.Subscription.DTO;
using NPS.public_intraday_api_example.Utilities;

namespace NPS.public_intraday_api_example
{
    class Program
    {
        private static ILog _logger =
            LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int DemoArea = 7;

        static void Main(string[] args)
        {
            // Authorize to get auth token
            var token = AuthorizeToSSOService();

            // Connect to trading service
            var tradingService = ConnectToTradingService(token);

            // Subscribe to topics
            SubscribeToServices(tradingService);
         
            // Send sample incorrect order reques to service:
            SendSampleIncorrectOrderRequest(tradingService);

            // Waiting for disconnect
            WaitForDisconnectFromTradingService(tradingService);

        }

        /// <summary>
        /// Returns the auth token after authorizing.
        /// </summary>
        /// <returns></returns>
        public static string AuthorizeToSSOService()
        {
            var credentials = ReadUserCredentials();
            var ssoSettings = ReadSSOSettings();
            var ssoService = new SSOService(ssoSettings);

            return ssoService.GetAuthToken(credentials.UserName, credentials.Password);
        }

        /// <summary>
        /// Creates instance of trading service and connects to it.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns>connected trading service</returns>
        public static TradingService ConnectToTradingService(string authToken)
        {
            var wsSettings = ReadWebSocketSettings();
            var credentials = ReadUserCredentials();
            var tradingService = new TradingService(wsSettings, credentials);

            tradingService.Connect(authToken);

            return tradingService;
        }

        /// <summary>
        /// Subscribes to different topics with connected TradingService
        /// </summary>
        /// <param name="tradingService"></param>
        public static void SubscribeToServices(TradingService tradingService)
        {
            if (!tradingService.IsConnected)
                throw new InvalidOperationException("Trading service not connected! Can't subscribe to any topics.");

            var apiVersion = ConfigurationManager.AppSettings["api-version"];

            tradingService.Subscribe(Subscription.Builder()
                .WithTopic(Topic.Ticker)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .Build(), TickerCallBack);

            tradingService.Subscribe(Subscription.Builder()
                .WithTopic(Topic.DeliveryAreas)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .WithArea(DemoArea)
                .Build(), DeliveryAreasCallBack);

            tradingService.Subscribe(Subscription.Builder()
                .WithTopic(Topic.OrderExecutionReport)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .Build(), OrderExecutionCallBack);

            tradingService.Subscribe(Subscription.Builder()
                .WithTopic(Topic.Configuration)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Empty)
                .Build(), ConfigurationCallBack);

            tradingService.Subscribe(Subscription.Builder()
                .WithTopic(Topic.Contracts)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .Build(), ContractsCallBack);

            tradingService.Subscribe(Subscription.Builder()
                .WithTopic(Topic.LocalView)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .WithArea(DemoArea)
                .WithIsGzipped(true)
                .Build(), LocalViewCallBack);

            tradingService.Subscribe(Subscription.Builder()
                .WithTopic(Topic.PrivateTrade)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .Build(), PrivateTradeCallBack);

            tradingService.Subscribe(Subscription.Builder()
                .WithTopic(Topic.PublicStatistics)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Conflated)
                .WithArea(DemoArea)
                .Build(), PublicStatisticsCallBack);

            tradingService.Subscribe(Subscription.Builder()
                .WithTopic(Topic.Capacities)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .WithArea(DemoArea)
                .Build(), CapacitiesCallBack);

            try
            {
                Thread.Sleep(3000);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        public static void SendSampleIncorrectOrderRequest(TradingService tradingService)
        {

            _logger.Info("Attempting to send an incorrect order, you will see the rejection message in the log.");
            var incorrectOrder = SampleIncorrectOrderRequest();
            tradingService.SendEntryOrderRequest(incorrectOrder);
        }

        public static void WaitForDisconnectFromTradingService(TradingService tradingService)
        {
            try
            {
                Thread.Sleep(3000);
                _logger.Fatal("Will wait for Enter to close all the subscriptons and disconnect.");
                Console.ReadLine();
                tradingService.SendLogoutCommand();

            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private static void CapacitiesCallBack(string messagecontent)
        {
            ShowMessage(messagecontent, "Capacities");
        }

        private static void PublicStatisticsCallBack(string messagecontent)
        {
            ShowMessage(messagecontent, "Public Statistics");
        }

        private static void PrivateTradeCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Private Trade");
        }

        private static void LocalViewCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Local View", true);
        }

        private static void ConfigurationCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Configuration");
        }

        private static void ContractsCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Contracts");
        }

        private static void OrderExecutionCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Order Execution Report");
        }

        private static void DeliveryAreasCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Delivery Areas");
        }

        private static void TickerCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Ticker");
        }


        private static OrderEntryRequest SampleIncorrectOrderRequest()
        {
            var request = new OrderEntryRequest()
            {
                requestId = Guid.NewGuid().ToString(),
                orders = new[]
                {
                    new OrderEntryRequest.Order()
                    {
                        clientOrderId = Guid.NewGuid().ToString() // User's own UID to track this order
                    },
                }
            };

            return request;
        }
        private static void ShowMessage(string message, string fromTopic, bool isGzipped = false)
        {
            _logger.Info($"Message from \"{fromTopic}\" topic:");

            var messageContent = isGzipped ? GzipCompressor.Decompress(message) : message;

            if (messageContent.Length > 500)
            {
                _logger.Info($"Message is {messageContent.Length} chars long. Only first 500 chars of JSON are shown.");
                _logger.Info($"JSON: {messageContent.Substring(0, 500)}");
            }
            else
            {
                _logger.Info($"JSON: {messageContent}");
            }

        }

        private static BasicCredentials ReadUserCredentials()
        {
            var credentials =  new BasicCredentials()
            {
                UserName = ConfigurationManager.AppSettings["sso-user"],
                Password = ConfigurationManager.AppSettings["sso-password"]
           
            };

            _logger.Info($"Credentials read from App.config. User: {credentials.UserName} Password: {credentials.Password}");

            return credentials;
        }

        private static SSOSettings ReadSSOSettings()
        {
            var ssoSettings =  new SSOSettings()
            {
                ClientId = ConfigurationManager.AppSettings["sso-clientId"],
                ClientSecret = ConfigurationManager.AppSettings["sso-clientSecret"],
                Protocol = ConfigurationManager.AppSettings["sso-protocol"],
                Host = ConfigurationManager.AppSettings["sso-host"],
                TokenUri = ConfigurationManager.AppSettings["sso-tokenUri"]
            };

            _logger.Info($"SSO settings read from App.config:");
            _logger.Info($"Client Id: {ssoSettings.ClientId}");
            _logger.Info($"Client Secret: {ssoSettings.ClientSecret}");
            _logger.Info($"Protocol: {ssoSettings.Protocol}");
            _logger.Info($"Host: {ssoSettings.Host}");
            _logger.Info($"Token URI: {ssoSettings.TokenUri}");

            return ssoSettings;
        }

        private static WebSocketSettings ReadWebSocketSettings()
        {
            var webSocketSettings =  new WebSocketSettings()
            {
                Host = ConfigurationManager.AppSettings["ws-host"],
                Port = Convert.ToInt32(ConfigurationManager.AppSettings["ws-port"]),
                Protocol = ConfigurationManager.AppSettings["ws-protocol"],
                Uri = ConfigurationManager.AppSettings["ws-uri"]


            };

            _logger.Info($"Web socket settings read from App.config:");
            _logger.Info($"WS Host: {webSocketSettings.Host}");
            _logger.Info($"WS Port: {webSocketSettings.Port}");
            _logger.Info($"WS Protocol: {webSocketSettings.Protocol}");
            _logger.Info($"WS Uri: {webSocketSettings.Host}");

            return webSocketSettings;
        }

    }
}