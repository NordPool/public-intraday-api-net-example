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
using NPS.ID.PublicApi.Models;
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
    public partial class Form1 : Form
    {
        private static ILog _logger =
            LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int DemoArea = 2;


        public Form1()
        {
            InitializeComponent();
        }

        private TradingService tradingService;

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                Log("Starting to connect trading service..");
                // Authorize to get auth token
                var token = await AuthorizeToSSOService();
                Log($"Got token {token}");
                // Connect to trading service
                tradingService = ConnectToTradingService(token);
                Log($"Connected to Trading Service");
                // Subscribe to topics
                SubscribeToServices(tradingService);
                Log($"Subscribed to services..");
                //// Send sample incorrect order reques to service:
                //SendSampleIncorrectOrderRequest(tradingService);

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
                .WithTopic(Topic.HeartBeatPing)
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


        public void SendSampleOrderRequest(TradingService tradingService)
        {

            Log("Attempting to send an order");
            var incorrectOrder = SampleIncorrectOrderRequest();
            tradingService.SendEntryOrderRequest(incorrectOrder);
        }

        public void SendSampleIncorrectOrderRequest(TradingService tradingService)
        {

            Log("Attempting to send an incorrect order, you will see the rejection message in the log.");
            var incorrectOrder = SampleIncorrectOrderRequest();
            tradingService.SendEntryOrderRequest(incorrectOrder);
        }

        private void HeartbeatCallBack(string messagecontent)
        {
            ShowMessage(messagecontent, "Heartbeat");
        }

        private void CapacitiesCallBack(string messagecontent)
        {
            ShowMessage(messagecontent, "Capacities");

        }

        private void PublicStatisticsCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Public Statistics");
            var publicStatistics = JsonHelper.DeserializeData<List<PublicStatisticRow>>(messageContent);
        }

        private void PrivateTradeCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Private Trade");
        }

        private void LocalViewCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Local View", true);
            var localViewData= JsonHelper.DeserializeData<List<LocalViewRow>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(localViewData));

            var genLocalViewData = JsonHelper.DeserializeData<List<Models.Generated.LocalViewRow>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(genLocalViewData));
        }

        private void ConfigurationCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Configuration");
            var configurations = JsonHelper.DeserializeData<List<ConfigurationRow>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(configurations));

            //var configurationsGen = JsonHelper.DeserializeData<List<NPS.ID.PublicApi.Models.Generated.ConfigurationRow>>(messageContent);
            //Log(JsonHelper.SerializeObjectPrettyPrinted(configurationsGen));
        }

        private void ContractsCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Contracts");
            var contracts = JsonHelper.DeserializeData<List<ContractRow>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(contracts));
        }

        private void OrderExecutionCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Order Execution Report");
            var orderExecutionReports = JsonHelper.DeserializeData<List<OrderExecutionReport>>(messageContent);
            Log(JsonHelper.SerializeObjectPrettyPrinted(orderExecutionReports));
        }

        private void DeliveryAreasCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Delivery Areas");

        }

        private void TickerCallBack(string messageContent)
        {
            ShowMessage(messageContent, "Ticker");
            var ticker = JsonHelper.DeserializeData<List<PublicTradeRow>>(messageContent);
        }



        private static OrderEntryRequest SampleOrderRequest()
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

        private static OrderEntryRequest SampleIncorrectOrderRequest()
        {
            var request = new OrderEntryRequest()
            {
                RequestId = Guid.NewGuid().ToString(),
                Orders = new List<OrderEntry>
                {
                    new OrderEntry()
                    {
                        ClientOrderId = "Something"
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

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                SendSampleIncorrectOrderRequest(tradingService);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void buttonGenerateSchemas_Click(object sender, EventArgs e)
        {
            try
            {

                var types = new Type[] {typeof(ConfigurationRow),
                    typeof(HeartbeatMessage),
                    typeof(DeliveryAreaRow),
                    typeof(ContractRow),
                    typeof(LocalViewRow),
                    typeof(PublicStatisticRow),
                    typeof(PublicTradeRow),
                    typeof(CapacityRow),
                    typeof(OrderEntryRequest),
                    typeof(OrderModificationRequest),
                    typeof(OrderExecutionReport),
                    typeof(PrivateTradeRow)
                };

                this.textBoxLog.Text = "";

                var path = @"C:\NordPool\public-intraday-api-net-example\NPS.ID.PublicApi.Models\json-schema";

                foreach (var jsonType in types)
                {
                    var jsonSchemaGenerator = new JSchemaGenerator();
                    jsonSchemaGenerator.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    jsonSchemaGenerator.GenerationProviders.Add(new StringEnumGenerationProvider());
                    jsonSchemaGenerator.SchemaLocationHandling = SchemaLocationHandling.Definitions;
                    jsonSchemaGenerator.SchemaPropertyOrderHandling = SchemaPropertyOrderHandling.Default;
                    jsonSchemaGenerator.SchemaReferenceHandling = SchemaReferenceHandling.Objects;

                    var myType = jsonType;
                    var schema = jsonSchemaGenerator.Generate(myType);

                    schema.Title = myType.Name; // this doesn't seem to get done within the generator
                    var writer = new StringWriter();
                    var jsonTextWriter = new JsonTextWriter(writer);
                    schema.WriteTo(jsonTextWriter);
                    dynamic parsedJson = JsonConvert.DeserializeObject(writer.ToString());
                    var prettyString = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                    File.WriteAllText(Path.Combine(path, $"{jsonType.Name}.json"), prettyString);
                    this.textBoxLog.Text += prettyString + Environment.NewLine;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private async void buttonGenerateCS_Click(object sender, EventArgs e)
        {
            try
            {
                var schemaFolder = new DirectoryInfo(@"C:\NordPool\public-intraday-api-net-example\NPS.ID.PublicApi.Models\json-schema\");
                foreach (var schemaFile in schemaFolder.EnumerateFiles())
                {
                    var contents = File.ReadAllText(schemaFile.FullName);
                    var schema = await JsonSchema4.FromJsonAsync(contents);
                    var generator = new CSharpGenerator(schema);
                    generator.Settings.ArrayType = "System.Collections.Generic.List";
                    generator.Settings.Namespace = "NPS.ID.PublicApi.Models.Generated";
                    generator.Settings.DateTimeType = "System.DateTimeOffset";
                    generator.Settings.ClassStyle = CSharpClassStyle.Poco;
                    
                                        
                    var csCode = generator.GenerateFile();
                    File.WriteAllText(Path.Combine(@"C:\NordPool\public-intraday-api-net-example\NPS.ID.PublicApi.Models\cs", $"{schema.Title}.cs"), csCode);
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }
    }
}
