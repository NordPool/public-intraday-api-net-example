/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool’s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Serialization;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using NPS.ID.PublicApi.Client.Connection;
using NPS.ID.PublicApi.Client.Security;
using NPS.ID.PublicApi.Client.Subscription;
using NPS.ID.PublicApi.Client.Utilities;
using NPS.ID.PublicApi.Models.v1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPS.ID.PublicApi.Client.WinFormsExample
{
    public partial class MainForm : Form
    {
        private static ILog _logger =
            LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int DemoArea = 2;

        private ContractRow sampleContract = null;
        private ConfigurationRow currentConfiguration = null;
        private OrderEntryRequest lastSentOrder = null;


        public MainForm()
        {
            InitializeComponent();
        }

        private TradingService tradingService;


        /// <summary>
        /// Connect to Nord Pool Intraday
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                Log("Starting to connect Nord Pool Intraday.");
                // Authorize to get auth token
                var token = await AuthorizeToSSOService();
                Log($"Got token {token}");
                // Connect to trading service
                tradingService = ConnectToTradingService(token);
                Log($"Connected to Trading Service");
                // Subscribe to topics
                SubscribeToServices(tradingService);
                Log($"Subscribed to services..");

                this.buttonLogout.Enabled = true;
                this.buttonSendOrderEntry.Enabled = true;
                this.buttonSendOrderModification.Enabled = true;
                this.buttonSendTradeRecall.Enabled = true;
                this.buttonConnect.Enabled = false;

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }
        delegate void SetTextCallback(string text);
        private void Log(string text)
        {
            if (this.textBoxLog.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(Log);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBoxLog.Text += text + Environment.NewLine;
            }

        }

        /// <summary>
        /// Returns the auth token after authorizing.
        /// </summary>
        /// <returns></returns>
        public async Task<string> AuthorizeToSSOService()
        {
            var credentials = ReadUserCredentials();
            var ssoSettings = ReadSSOSettings();
            var ssoService = new SSOService(ssoSettings);

            return await ssoService.GetAuthToken(credentials.UserName, credentials.Password);
        }

        /// <summary>
        /// Creates instance of trading service and connects to it.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns>connected trading service</returns>
        public TradingService ConnectToTradingService(string authToken)
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
        public void SubscribeToServices(TradingService tradingService)
        {
            if (!tradingService.IsConnected)
                throw new InvalidOperationException("Trading service not connected! Can't subscribe to any topics.");

            var apiVersion = ConfigurationManager.AppSettings["api-version"];

            tradingService.Subscribe(Subscription.Subscription.Builder()
                .WithTopic(Topic.Ticker)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .Build(), TickerCallBack);

            tradingService.Subscribe(Subscription.Subscription.Builder()
                .WithTopic(Topic.DeliveryAreas)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .WithArea(DemoArea)
                .Build(), DeliveryAreasCallBack);

            tradingService.Subscribe(Subscription.Subscription.Builder()
                .WithTopic(Topic.OrderExecutionReport)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .Build(), OrderExecutionCallBack);

            tradingService.Subscribe(Subscription.Subscription.Builder()
                .WithTopic(Topic.Configuration)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Empty)
                .Build(), ConfigurationCallBack);

            tradingService.Subscribe(Subscription.Subscription.Builder()
                .WithTopic(Topic.Contracts)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .Build(), ContractsCallBack);

            tradingService.Subscribe(Subscription.Subscription.Builder()
                .WithTopic(Topic.LocalView)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .WithArea(DemoArea)
                .WithIsGzipped(true)
                .Build(), LocalViewCallBack);

            tradingService.Subscribe(Subscription.Subscription.Builder()
                .WithTopic(Topic.PrivateTrade)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .Build(), PrivateTradeCallBack);

            tradingService.Subscribe(Subscription.Subscription.Builder()
                .WithTopic(Topic.PublicStatistics)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Conflated)
                .WithArea(DemoArea)
                .Build(), PublicStatisticsCallBack);

            tradingService.Subscribe(Subscription.Subscription.Builder()
                .WithTopic(Topic.Capacities)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .WithArea(DemoArea)
                .Build(), CapacitiesCallBack);

            tradingService.Subscribe(Subscription.Subscription.Builder()
                .WithTopic(Topic.HeartbeatPing)
                .WithVersion(apiVersion)
                .WithSubscriptionType(SubscriptionType.Streaming)
                .WithArea(DemoArea)
                .Build(), HeartbeatCallBack);

            try
            {
                Thread.Sleep(3000);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private void HeartbeatCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Heartbeat");
            var heartbeatData = JsonHelper.DeserializeData<List<PublicStatisticRow>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(heartbeatData));
        }

        private void CapacitiesCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Capacities");
            var capacitiesData = JsonHelper.DeserializeData<List<CapacityRow>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(capacitiesData));
        }

        private void PublicStatisticsCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Public Statistics");
            var publicStatisticsData = JsonHelper.DeserializeData<List<PublicStatisticRow>>(messageContent);
            //Log(JsonHelper.SerializeObjectPrettyPrinted(publicStatisticsData));
        }

        private void PrivateTradeCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Private Trade");
            var privateTradesData = JsonHelper.DeserializeData<List<PrivateTradeRow>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(privateTradesData));
        }

        private void LocalViewCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Local View", true);
            var localViewData = JsonHelper.DeserializeData<List<LocalViewRow>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(localViewData));
        }

        private void ConfigurationCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Configuration");
            var configurationData = JsonHelper.DeserializeData<List<ConfigurationRow>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(configurationData));
            if (currentConfiguration == null)
                currentConfiguration = configurationData.FirstOrDefault();
        }

        private void ContractsCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Contracts");
            var contractsData = JsonHelper.DeserializeData<List<ContractRow>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(contractsData));
            if (sampleContract == null)
            {
                sampleContract = contractsData.FirstOrDefault(r => r.State == StateEnum.ACTI && r.DlvryStart > DateTimeOffset.Now.AddHours(3) && r.DlvryStart > DateTimeOffset.Now.AddHours(5));
                Log($"Sample contract: {Environment.NewLine}{JsonHelper.SerializeObjectPrettyPrinted(contractsData)}");
            }
        }

        private void OrderExecutionCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Order Execution Report");
            var orderExecutionsData = JsonHelper.DeserializeData<List<OrderExecutionReport>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(orderExecutionsData));
        }

        private void DeliveryAreasCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Delivery Areas");
            var deliveryAreasData = JsonHelper.DeserializeData<List<DeliveryAreaRow>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(deliveryAreasData));

        }

        private void TickerCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Ticker");
            var tickerData = JsonHelper.DeserializeData<List<PublicTradeRow>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(tickerData));
        }



        private static OrderEntryRequest SampleIncorrectOrderRequest()
        {
            var request = new OrderEntryRequest()
            {
                RequestId = Guid.NewGuid().ToString(),
                RejectPartially = false,

                Orders = new List<OrderEntry>
                {
                    new OrderEntry()
                    {
                        ClientOrderId = Guid.NewGuid().ToString(),
                         PortfolioId = "Z00001-5"

                    },
                }
            };

            return request;
        }

        private OrderEntryRequest SampleOrderRequest()
        {
            var portFolio = currentConfiguration.Portfolios.Last();
            var request = new OrderEntryRequest()
            {
                RequestId = Guid.NewGuid().ToString(),
                RejectPartially = false,
                Orders = new List<OrderEntry>
                {
                    new OrderEntry()
                    {
                        ClientOrderId = Guid.NewGuid().ToString(),
                        PortfolioId  = portFolio.Id,
                        Side =  SideEnum.SELL,
                        ContractIds = new List<string> { sampleContract.ContractId },
                        OrderType = OrderTypeEnum.LIMIT,
                        Quantity = 3000,
                        State =  StateEnum.ACTI,
                        UnitPrice = 2500,
                        TimeInForce =  TimeInForceEnum.GFS,
                        DeliveryAreaId = portFolio.Areas.First().AreaId,
                        //ExecutionRestriction = OrderEntryExecutionRestriction.AON,
                        //ExpireTime = DateTimeOffset.Now.AddHours(6)
                    },
                }
            };

            return request;
        }
        private void ShowMessage(string message, string fromTopic, bool isGzipped = false)
        {
            Log($"Message from \"{fromTopic}\" topic");

            //var messageContent = isGzipped ? GzipCompressor.Decompress(message) : message;
            //Log($"JSON: {messageContent}");
        }

        private BasicCredentials ReadUserCredentials()
        {
            var credentials = new BasicCredentials()
            {
                UserName = ConfigurationManager.AppSettings["sso-user"],
                Password = ConfigurationManager.AppSettings["sso-password"]

            };

            Log($"Credentials read from App.config. User: {credentials.UserName} Password: {credentials.Password}");

            return credentials;
        }

        private SSOSettings ReadSSOSettings()
        {
            var ssoSettings = new SSOSettings()
            {
                ClientId = ConfigurationManager.AppSettings["sso-clientId"],
                ClientSecret = ConfigurationManager.AppSettings["sso-clientSecret"],
                Protocol = ConfigurationManager.AppSettings["sso-protocol"],
                Host = ConfigurationManager.AppSettings["sso-host"],
                TokenUri = ConfigurationManager.AppSettings["sso-tokenUri"]
            };

            Log($"SSO settings read from App.config:");
            Log($"Client Id: {ssoSettings.ClientId}");
            Log($"Client Secret: {ssoSettings.ClientSecret}");
            Log($"Protocol: {ssoSettings.Protocol}");
            Log($"Host: {ssoSettings.Host}");
            Log($"Token URI: {ssoSettings.TokenUri}");

            return ssoSettings;
        }

        private WebSocketSettings ReadWebSocketSettings()
        {
            var webSocketSettings = new WebSocketSettings()
            {
                Host = ConfigurationManager.AppSettings["ws-host"],
                Port = Convert.ToInt32(ConfigurationManager.AppSettings["ws-port"]),
                Protocol = ConfigurationManager.AppSettings["ws-protocol"],
                Uri = ConfigurationManager.AppSettings["ws-uri"]
            };

            Log($"Web socket settings read from App.config:");
            Log($"WS Host: {webSocketSettings.Host}");
            Log($"WS Port: {webSocketSettings.Port}");
            Log($"WS Protocol: {webSocketSettings.Protocol}");
            Log($"WS Uri: {webSocketSettings.Host}");

            return webSocketSettings;
        }

        private void buttonSendOrderEntry_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentConfiguration == null)
                {
                    MessageBox.Show("Missing login information. Add your credentials to app.config and restart the application");
                    return;
                }
                var order = SampleOrderRequest();

                var str = InputForm.ShowForm(JsonHelper.SerializeObjectPrettyPrinted(order));
                if (str == "")
                    return;

                order = JsonHelper.DeserializeData<OrderEntryRequest>(str);

                tradingService.SendEntryOrderRequest(order);
                Log($"Sent order:{Environment.NewLine}{JsonHelper.SerializeObjectPrettyPrinted(order)}");
                lastSentOrder = order;

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            this.textBoxLog.Text = "";
        }

        private void buttonLogout_Click(object sender, EventArgs e)
        {
            try
            {
                if (tradingService == null)
                {
                    return;
                }
                tradingService.SendLogoutCommand();

                this.buttonLogout.Enabled = false;
                this.buttonSendOrderEntry.Enabled = false;
                this.buttonSendOrderModification.Enabled = false;
                this.buttonSendTradeRecall.Enabled = false;
                this.buttonConnect.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void buttonSendOrderModification_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentConfiguration == null)
                {
                    MessageBox.Show("Missing login information. Add your credentials to app.config and restart the application");
                    return;
                }
                var request = new OrderModificationRequest()
                {
                    RequestId = Guid.NewGuid().ToString(),
                    OrderModificationType = OrderModificationTypeEnum.DEAC,
                    Orders = new List<OrderModification>()
                     {
                         new OrderModification()
                         {
                             OrderId = "",
                            ClientOrderId = lastSentOrder?.Orders.FirstOrDefault()?.ClientOrderId,
                            ClipPriceChange = (lastSentOrder?.Orders.FirstOrDefault()?.ClipPriceChange ?? 0),
                            ClipSize = (lastSentOrder?.Orders.FirstOrDefault()?.ClipSize ?? 0),
                            ContractIds = lastSentOrder?.Orders.FirstOrDefault()?.ContractIds,
                            ExecutionRestriction = lastSentOrder?.Orders.FirstOrDefault()?.ExecutionRestriction ?? ExecutionRestrictionEnum.AON, 
                            ExpireTime = lastSentOrder?.Orders.FirstOrDefault()?.ExpireTime ?? DateTimeOffset.MinValue,
                            OrderType = lastSentOrder?.Orders.FirstOrDefault()?.OrderType ?? OrderTypeEnum.LIMIT,
                            PortfolioId = lastSentOrder?.Orders.FirstOrDefault()?.PortfolioId,
                            Quantity = (lastSentOrder?.Orders.FirstOrDefault()?.Quantity ?? 0),
                            RevisionNo = 0,
                            Text = "",
                            TimeInForce =  lastSentOrder?.Orders.FirstOrDefault()?.TimeInForce ?? TimeInForceEnum.GFS,
                            UnitPrice = lastSentOrder?.Orders.FirstOrDefault()?.UnitPrice ?? 0
                         }
                     }
                };

                var str = InputForm.ShowForm(JsonHelper.SerializeObjectPrettyPrinted(request));
                if (str == "")
                    return;

                request = JsonHelper.DeserializeData<OrderModificationRequest>(str);
                tradingService.SendModificationOrderRequest(request);
                Log($"Sent order modification:{Environment.NewLine}{JsonHelper.SerializeObjectPrettyPrinted(request)}");


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void buttonSendTradeRecall_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentConfiguration == null)
                {
                    MessageBox.Show("Missing login information. Add your credentials to app.config and restart the application");
                    return;
                }
                var request = new TradeRecallRequest()
                {
                     RevisionNo = "",
                     TradeId = ""
                };

                var str = InputForm.ShowForm(JsonHelper.SerializeObjectPrettyPrinted(request));
                if (str == "")
                    return;

                request = JsonHelper.DeserializeData<TradeRecallRequest>(str);
                tradingService.SendTradeCancellationRequest(request);
                Log($"Sent trade recall:{Environment.NewLine}{JsonHelper.SerializeObjectPrettyPrinted(request)}");


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
