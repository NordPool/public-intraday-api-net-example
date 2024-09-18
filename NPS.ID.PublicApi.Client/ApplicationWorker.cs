using System.Text.Json;
using Extend;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nordpool.ID.PublicApi.v1;
using Nordpool.ID.PublicApi.v1.Contract;
using Nordpool.ID.PublicApi.v1.Order;
using Nordpool.ID.PublicApi.v1.Order.Request;
using Nordpool.ID.PublicApi.v1.Statistic;
using Nordpool.ID.PublicApi.v1.Throttling;
using NPS.ID.PublicApi.Client.Connection.Clients;
using NPS.ID.PublicApi.Client.Connection.Enums;
using NPS.ID.PublicApi.Client.Connection.Storage;
using NPS.ID.PublicApi.Client.Connection.Subscriptions;
using NPS.ID.PublicApi.Client.Connection.Subscriptions.Helpers;
using NPS.ID.PublicApi.Client.Connection.Subscriptions.Requests;
using NPS.ID.PublicApi.Client.Security.Options;

namespace NPS.ID.PublicApi.Client;

public class ApplicationWorker
{
    private readonly ILogger<ApplicationWorker> _logger;

    private const string Version = "v1";
    private const int DemoArea = 2; // 3 = Finland
    
    private readonly string _clientId = $"{Guid.NewGuid()}-dotnet-demo-client";

    private readonly StompClientFactory _clientFactory;
    private readonly SubscribeRequestBuilder _subscribeRequestBuilder;

    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly MemoryCacheProxy _memoryCacheProxy = new();

    public ApplicationWorker(
        ILogger<ApplicationWorker> logger,
        IOptions<CredentialsOptions> credentialsOptions,
        StompClientFactory clientFactory)
    {
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        _clientFactory = clientFactory;
        _subscribeRequestBuilder = SubscribeRequestBuilder.CreateBuilder(credentialsOptions.Value.UserName, Version);

        Console.CancelKeyPress += (_, eArgs) =>
        {
            _cancellationTokenSource.Cancel();
            eArgs.Cancel = true;
            Environment.Exit(0);
        };
    }

    public async Task RunAsync()
    {
        var marketDataClient = await
            _clientFactory.CreateAsync(WebSocketClientTarget.MARKET_DATA, _clientId, _cancellationTokenSource.Token);
        var tradingClient = await
            _clientFactory.CreateAsync(WebSocketClientTarget.TRADING, _clientId, _cancellationTokenSource.Token);

        // Set clients disconnection behaviour while closing app with CTRL + C keys
        _cancellationTokenSource.Token
            .Register(() =>
            {
                var marketDataClientDisconnectionTask = marketDataClient.DisconnectAsync(CancellationToken.None);
                var tradingClientDisconnectionTask = tradingClient.DisconnectAsync(CancellationToken.None);
                Task.WaitAll(marketDataClientDisconnectionTask, tradingClientDisconnectionTask);
            });
        
        // Delivery areas
        await SubscribeDeliveryAreasAsync(marketDataClient, _cancellationTokenSource.Token);

        // Configurations 
        await SubscribeConfigurationsAsync(tradingClient, _cancellationTokenSource.Token);
        
        // Order execution report
        await SubscribeOrderExecutionReportAsync(tradingClient,
            PublishingMode.STREAMING,
            _cancellationTokenSource.Token);
        
        // Contracts
        await SubscribeContractsAsync(marketDataClient, PublishingMode.CONFLATED,
            _cancellationTokenSource.Token);

        // Local views 
        await SubscribeLocalViewsAsync(marketDataClient, PublishingMode.CONFLATED,
            _cancellationTokenSource.Token);
        
        // Private trades
        await SubscribePrivateTradesAsync(tradingClient, PublishingMode.STREAMING,
            _cancellationTokenSource.Token);
        
        // Tickers 
        await SubscribeTickersAsync(marketDataClient, PublishingMode.CONFLATED,
            _cancellationTokenSource.Token);
        
        // MyTickers 
        await SubscribeMyTickersAsync(marketDataClient, PublishingMode.CONFLATED,
            _cancellationTokenSource.Token);
        
        // Public statistics 
        await SubscribePublicStatisticsAsync(marketDataClient, PublishingMode.CONFLATED,
            _cancellationTokenSource.Token);
        
        // Throttling limits
        await SubscribeThrottlingLimitsAsync(tradingClient,
            PublishingMode.CONFLATED,
            _cancellationTokenSource.Token);
        
        // Capacities 
        await SubscribeCapacitiesAsync(marketDataClient, PublishingMode.CONFLATED,
            _cancellationTokenSource.Token);

        // Order 
        // We wait some time in hope to get some example contracts and configurations that are needed for preparing example order request
        Thread.Sleep(5000);
        await SendOrderEntryRequestAsync(tradingClient, 
            _cancellationTokenSource.Token);
        // Wait before order modification
        Thread.Sleep(5000);
        await SendOrderModificationRequestAsync(tradingClient,
            _cancellationTokenSource.Token);
        
         // Wait before invalid order request
         Thread.Sleep(5000);
         await SendInvalidOrderEntryRequestAsync(tradingClient,
             _cancellationTokenSource.Token);
         // Wait before invalid order modification request
         Thread.Sleep(5000);
         await SendInvalidOrderModificationRequestAsync(tradingClient,
             _cancellationTokenSource.Token);

        _cancellationTokenSource.Token.WaitHandle.WaitOne();
    }

    private async Task SubscribeDeliveryAreasAsync(IClient client,
        CancellationToken cancellationToken)
    {
        var deliveryAreasRequest = _subscribeRequestBuilder.CreateDeliveryAreas();
        var subscription = await client.SubscribeAsync<DeliveryAreaRow>(deliveryAreasRequest, cancellationToken);
        ReadSubscriptionChannel(client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeConfigurationsAsync(IClient client, CancellationToken cancellationToken)
    {
        var configurationsSubscription = _subscribeRequestBuilder.CreateConfiguration();
        var subscription = await client.SubscribeAsync<ConfigurationRow>(configurationsSubscription, cancellationToken);
        ReadSubscriptionChannel(client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeOrderExecutionReportAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var orderExecutionReportSubscription = _subscribeRequestBuilder.CreateOrderExecutionReport(publishingMode);
        var subscription =
            await client.SubscribeAsync<OrderExecutionReport>(orderExecutionReportSubscription, cancellationToken);
        ReadSubscriptionChannel(client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeContractsAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var contractsSubscription = _subscribeRequestBuilder.CreateContracts(publishingMode);
        var subscription = await client.SubscribeAsync<ContractRow>(contractsSubscription, cancellationToken);
        ReadSubscriptionChannel(client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeLocalViewsAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var localViewsSubscription = _subscribeRequestBuilder.CreateLocalViews(publishingMode, DemoArea);
        var subscription = await client.SubscribeAsync<LocalViewRow>(localViewsSubscription, cancellationToken);
        ReadSubscriptionChannel(client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribePrivateTradesAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var privateTradesSubscription = _subscribeRequestBuilder.CreatePrivateTrades(publishingMode);
        var subscription = await client.SubscribeAsync<PrivateTradeRow>(privateTradesSubscription, cancellationToken);
        ReadSubscriptionChannel(client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeTickersAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var tickersSubscription = _subscribeRequestBuilder.CreateTicker(publishingMode);
        var subscription = await client.SubscribeAsync<PublicTradeRow>(tickersSubscription, cancellationToken);
        ReadSubscriptionChannel(client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeMyTickersAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var myTickersSubscription = _subscribeRequestBuilder.CreateMyTicker(publishingMode);
        var subscription = await client.SubscribeAsync<PublicTradeRow>(myTickersSubscription, cancellationToken);
        ReadSubscriptionChannel(client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribePublicStatisticsAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var publicStatisticsSubscription = _subscribeRequestBuilder.CreatePublicStatistics(publishingMode, DemoArea);
        var subscription =
            await client.SubscribeAsync<PublicStatisticRow>(publicStatisticsSubscription, cancellationToken);
        ReadSubscriptionChannel(client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeThrottlingLimitsAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var throttlingLimitsSubscription = _subscribeRequestBuilder.CreateThrottlingLimits(publishingMode);
        var subscription =
            await client.SubscribeAsync<ThrottlingLimitMessage>(throttlingLimitsSubscription, cancellationToken);
        ReadSubscriptionChannel(client.ClientTarget, subscription, cancellationToken);
        
        // Set automatic unsubscription of throttling limit topic after 10s
        _ = Task.Run(async () =>
        {
            await Task.Delay(10000, cancellationToken);
            await client.UnsubscribeAsync(subscription.Id, cancellationToken);
        }, cancellationToken);
    }

    private async Task SubscribeCapacitiesAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var contractsSubscription = _subscribeRequestBuilder.CreateCapacities(publishingMode, DemoArea);
        var subscription = await client.SubscribeAsync<CapacityRow>(contractsSubscription, cancellationToken);
        ReadSubscriptionChannel(client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SendOrderEntryRequestAsync(IClient client, CancellationToken cancellationToken)
    {
        var exampleData = GetExampleContractPortfolioAndArea(client);
        if (exampleData is null)
        {
            return;
        }

        var orderRequest = new OrderEntryRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            RejectPartially = false,
            Orders =
            [
                new OrderEntry
                {
                    Text = "New order",
                    ClientOrderId = Guid.NewGuid().ToString(),
                    PortfolioId = exampleData.Value.PortfolioId,
                    Side = OrderSide.SELL,
                    ContractIds = [exampleData.Value.ContractId],
                    OrderType = OrderType.LIMIT,
                    Quantity = 3000,
                    State = OrderState.ACTI,
                    UnitPrice = 2500,
                    TimeInForce = TimeInForce.GFS,
                    DeliveryAreaId = exampleData.Value.AreaId,
                    ExecutionRestriction = ExecutionRestriction.NON,
                    ExpireTime = DateTimeOffset.Now.AddHours(6)
                }
            ]
        };

        // Store created order in simple cache storage for order modification request
        _memoryCacheProxy.SetCache([orderRequest]);

        _logger.LogInformation("[{clientTarget}] Attempting to send correct order request.", client.ClientTarget);
        await client.SendAsync(orderRequest, DestinationHelper.ComposeDestination(Version,"orderEntryRequest"), cancellationToken);
    }

    private async Task SendOrderModificationRequestAsync(IClient client, CancellationToken cancellationToken)
    {
        // Get last created order for update purpose
        var lastOrder = _memoryCacheProxy.GetFromCache<OrderEntryRequest>()
            .LastOrDefault();
        if (lastOrder is null)
        {
            _logger.LogInformation("[{clientTarget}] No valid order to be used for order modification has been found!",
                client.ClientTarget);
            return;
        }
        var lastOrderEntry = lastOrder!.Orders.Single();

        // Get last order execution report response for above order request (OrderId required for order modification request)
        var lastOrderExecutionReport = _memoryCacheProxy.GetFromCache<OrderExecutionReport>()
            .LastOrDefault(oer => oer.Orders.Count == 1 && oer.Orders.Single().ClientOrderId == lastOrder.Orders.Single().ClientOrderId);
        if (lastOrderExecutionReport is null || lastOrderExecutionReport.Orders.IsNullOrEmpty())
        {
            _logger.LogInformation("[{clientTarget}] No valid order execution report to be used for order modification has been found!",
                client.ClientTarget);
            return;
        }

        var lastOrderExecutionReportOrderEntry = lastOrderExecutionReport!.Orders.Single();

        var orderModificationRequest = new OrderModificationRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            OrderModificationType = OrderModificationType.DEAC,
            Orders =
            [
                new OrderModification
                {
                    RevisionNo = 0L,
                    OrderId = lastOrderExecutionReportOrderEntry.OrderId,
                    Text = "Modified order",
                    ClientOrderId = lastOrderEntry.ClientOrderId,
                    PortfolioId = lastOrderEntry.PortfolioId,
                    ContractIds = lastOrderEntry.ContractIds,
                    OrderType = lastOrderEntry.OrderType,
                    Quantity = lastOrderEntry.Quantity,
                    UnitPrice = lastOrderEntry.UnitPrice,
                    TimeInForce = lastOrderEntry.TimeInForce,
                    ExecutionRestriction = lastOrderEntry.ExecutionRestriction,
                    ExpireTime = lastOrderEntry.ExpireTime,
                    ClipSize = lastOrderEntry.ClipSize,
                    ClipPriceChange = lastOrderEntry.ClipPriceChange
                }
            ]
        };

        _logger.LogInformation("[{clientTarget}] Attempting to send an correct order modification request.",
            client.ClientTarget);
        await client.SendAsync(orderModificationRequest, DestinationHelper.ComposeDestination(Version,"orderModificationRequest"), cancellationToken);
    }

    private async Task SendInvalidOrderEntryRequestAsync(IClient client,
        CancellationToken cancellationToken)
    {
        var exampleData = GetExampleContractPortfolioAndArea(client);
        if (exampleData is null)
        {
            return;
        }
        
        // Invalid order request - missing order details, detailed error will be logged as a part of order execution report response
        var invalidOrderRequest = new OrderEntryRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            RejectPartially = false,
            Orders =
            [
                new OrderEntry
                {
                    ClientOrderId = Guid.NewGuid().ToString(),
                    PortfolioId = exampleData.Value.PortfolioId
                }
            ]
        };

        _logger.LogInformation("[{clientTarget}] Attempting to send incorrect order request.", client.ClientTarget);
        await client.SendAsync(invalidOrderRequest, DestinationHelper.ComposeDestination(Version,"orderEntryRequest"), cancellationToken);
    }

    private async Task SendInvalidOrderModificationRequestAsync(IClient client,
        CancellationToken cancellationToken)
    {
        var invalidOrderModificationRequest = new OrderModificationRequest
        {
            RequestId = Guid.NewGuid().ToString(),
            OrderModificationType = OrderModificationType.DEAC,
            Orders =
            [
                new OrderModification
                {
                    ClientOrderId = Guid.NewGuid().ToString()
                }
            ]
        };

        _logger.LogInformation("[{clientTarget}] Attempting to send an incorrect order modification request.",
            client.ClientTarget);
        await client.SendAsync(invalidOrderModificationRequest, DestinationHelper.ComposeDestination(Version,"orderModificationRequest"), cancellationToken);
    }

    private (string ContractId, string PortfolioId, long AreaId)? GetExampleContractPortfolioAndArea(IClient client)
    {
        var exampleContracts = _memoryCacheProxy
            .GetFromCache<ContractRow>()
            .Where(c => c.ProductType != ProductType.CUSTOM_BLOCK && c.DlvryAreaState.Any(s => s.State == ContractState.ACTI))
            .ToList();
        if (exampleContracts.IsNullOrEmpty())
        {
            _logger.LogWarning(
                "[{clientTarget}] No valid contract to be used for order creation has been found in cache!",
                client.ClientTarget);
            return default;
        }

        var exampleRandomContract =  exampleContracts
            .ElementAtOrDefault(Random.Shared.Next(0, exampleContracts.Count));

        var exampleAreas = exampleRandomContract!
            .DlvryAreaState
            .Where(s => s.State == ContractState.ACTI);
        
        var examplePortfolios = _memoryCacheProxy.GetFromCache<ConfigurationRow>()
            .SelectMany(c => c.Portfolios)
            .Where(p => p.Areas.Any(a => exampleAreas.Any(s => s.DlvryAreaId == a.AreaId)))
            .ToList();
        
        var exampleRandomPortfolioForContract = examplePortfolios
            .ElementAtOrDefault(Random.Shared.Next(0, examplePortfolios.Count));
        if (exampleRandomPortfolioForContract is null)
        {
            _logger.LogWarning(
                "[{clientTarget}] No valid portfolio to be used for order creation has been found in cache!.",
                client.ClientTarget);
            return default;
        }

        var deliveryAreaPortfolio = exampleRandomPortfolioForContract.Areas
            .First(a => exampleAreas.Any(s => s.DlvryAreaId == a.AreaId));

        return (exampleRandomContract.ContractId, exampleRandomPortfolioForContract.Id, deliveryAreaPortfolio.AreaId);
    }
    
    private void ReadSubscriptionChannel<TValue>(WebSocketClientTarget clientTarget,
        ISubscription<TValue> subscription, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var channelReader = subscription.OutputChannel;
            while (await channelReader.WaitToReadAsync(cancellationToken))
            {
                if (!channelReader.TryRead(out var message))
                {
                    continue;
                }

                _memoryCacheProxy.SetCache(message.Data.ToList());
                var responseString = JsonSerializer.Serialize(message);

                // Trimming response content
                responseString = responseString.Length > 250
                    ? responseString[..250] + "..."
                    : responseString;

                _logger.LogInformation(
                    "[{clientTarget}][Frame({SubscriptionId}):Response] {ResponseString}", clientTarget,
                    subscription.Id, responseString);
            }
        }, cancellationToken);
    }
}