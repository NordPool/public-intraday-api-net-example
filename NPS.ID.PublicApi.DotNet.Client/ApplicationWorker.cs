using Extend;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nordpool.ID.PublicApi.v1;
using Nordpool.ID.PublicApi.v1.Contract;
using Nordpool.ID.PublicApi.v1.Order;
using Nordpool.ID.PublicApi.v1.Order.Request;
using Nordpool.ID.PublicApi.v1.Portfolio;
using Nordpool.ID.PublicApi.v1.Statistic;
using Nordpool.ID.PublicApi.v1.Throttling;
using NPS.ID.PublicApi.DotNet.Client.Connection.Clients;
using NPS.ID.PublicApi.DotNet.Client.Connection.Enums;
using NPS.ID.PublicApi.DotNet.Client.Connection.Storage;
using NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions;
using NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions.Helpers;
using NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions.Requests;
using NPS.ID.PublicApi.DotNet.Client.Security.Options;

namespace NPS.ID.PublicApi.DotNet.Client;

public class ApplicationWorker
{
    private readonly ILogger<ApplicationWorker> _logger;

    private readonly string _version = "v1";
    private readonly int _demoArea = 2; // 3 = Finland
    private readonly string _clientId = $"{Guid.NewGuid()}-dotnet-demo-client";

    private readonly IGenericClientFactory _genericClientFactory;
    private readonly SubscribeRequestBuilder _subscribeRequestBuilder;

    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly SimpleCacheStorage _simpleCacheStorage = new();

    public ApplicationWorker(
        ILogger<ApplicationWorker> logger,
        IOptions<CredentialsOptions> credentialsOptions,
        IGenericClientFactory genericClientFactory)
    {
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        _genericClientFactory = genericClientFactory;
        _subscribeRequestBuilder = SubscribeRequestBuilder.CreateBuilder(credentialsOptions.Value.UserName, _version);

        Console.CancelKeyPress += (_, eArgs) =>
        {
            _cancellationTokenSource.Cancel();
            eArgs.Cancel = true;
            Environment.Exit(0);
        };
    }

    public async Task RunAsync(string[] args)
    {
        var pmdClient =
            await CreateClientAsync(WebSocketClientTarget.PMD, _cancellationTokenSource.Token);
        var middlewareClient =
            await CreateClientAsync(WebSocketClientTarget.Middleware, _cancellationTokenSource.Token);

        // Delivery areas
        await SubscribeDeliveryAreasAsync(pmdClient, _cancellationTokenSource.Token);

        // Configurations 
        await SubscribeConfigurationsAsync(middlewareClient, _cancellationTokenSource.Token);
        
        // Order execution report
        await SubscribeOrderExecutionReportAsync(middlewareClient,
            PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        
        // Contracts
        await SubscribeContractsAsync(pmdClient, PublishingMode.Conflated,
            _cancellationTokenSource.Token);

        // Local views 
        await SubscribeLocalViewsAsync(pmdClient, PublishingMode.Conflated,
            _cancellationTokenSource.Token);
        
        // Private trades
        await SubscribePrivateTradesAsync(middlewareClient, PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        
        // Tickers 
        await SubscribeTickersAsync(pmdClient, PublishingMode.Conflated,
            _cancellationTokenSource.Token);
        
        // MyTickers 
        await SubscribeMyTickersAsync(pmdClient, PublishingMode.Conflated,
            _cancellationTokenSource.Token);
        
        // Public statistics 
        await SubscribePublicStatisticsAsync(pmdClient, PublishingMode.Conflated,
            _cancellationTokenSource.Token);
        
        // Throttling limits
        await SubscribeThrottlingLimitsAsync(middlewareClient,
            PublishingMode.Conflated,
            _cancellationTokenSource.Token);
        
        // Capacities 
        await SubscribeCapacitiesAsync(pmdClient, PublishingMode.Conflated,
            _cancellationTokenSource.Token);

        // Order 
        // We wait some time in hope to get some example contracts and configurations that are needed for preparing example order request
        Thread.Sleep(5000);
        await SendOrderRequestAsync(middlewareClient, 
            _cancellationTokenSource.Token);
        // Wait before order modification
        Thread.Sleep(5000);
        await SendOrderModificatonRequest(middlewareClient,
            _cancellationTokenSource.Token);
        
         // Wait before invalid order request
         Thread.Sleep(5000);
         await SendInvalidOrderRequestAsync(middlewareClient,
             _cancellationTokenSource.Token);
         // Wait before invalid order modification request
         Thread.Sleep(5000);
         await SendInvalidOrderModificatonRequest(middlewareClient,
             _cancellationTokenSource.Token);

        _cancellationTokenSource.Token.WaitHandle.WaitOne();
    }

    private async Task<IClient> CreateClientAsync(WebSocketClientTarget clientTarget,
        CancellationToken cancellationToken)
    {
        return await
            _genericClientFactory.CreateAsync(_clientId, clientTarget, cancellationToken);
    }

    private async Task SubscribeDeliveryAreasAsync(IClient client,
        CancellationToken cancellationToken)
    {
        var deliveryAreasRequest = _subscribeRequestBuilder.CreateDeliveryAreas();
        var subscription = await client.SubscribeAsync<DeliveryAreaRow>(deliveryAreasRequest, cancellationToken);
        await ReadSubscriptionChannel(client, client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeConfigurationsAsync(IClient client, CancellationToken cancellationToken)
    {
        var configurationsSubscription = _subscribeRequestBuilder.CreateConfiguration();
        var subscription = await client.SubscribeAsync<ConfigurationRow>(configurationsSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeOrderExecutionReportAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var orderExecutionReportSubscription = _subscribeRequestBuilder.CreateOrderExecutionReport(publishingMode);
        var subscription =
            await client.SubscribeAsync<OrderExecutionReport>(orderExecutionReportSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeContractsAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var contractsSubscription = _subscribeRequestBuilder.CreateContracts(publishingMode);
        var subscription = await client.SubscribeAsync<ContractRow>(contractsSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeLocalViewsAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var localViewsSubscription = _subscribeRequestBuilder.CreateLocalViews(publishingMode, _demoArea);
        var subscription = await client.SubscribeAsync<LocalViewRow>(localViewsSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribePrivateTradesAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var privateTradesSubscription = _subscribeRequestBuilder.CreatePrivateTrades(publishingMode);
        var subscription = await client.SubscribeAsync<PrivateTradeRow>(privateTradesSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeTickersAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var tickersSubscription = _subscribeRequestBuilder.CreateTicker(publishingMode);
        var subscription = await client.SubscribeAsync<PublicTradeRow>(tickersSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeMyTickersAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var myTickersSubscription = _subscribeRequestBuilder.CreateMyTicker(publishingMode);
        var subscription = await client.SubscribeAsync<PublicTradeRow>(myTickersSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribePublicStatisticsAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var publicStatisticsSubscription = _subscribeRequestBuilder.CreatePublicStatistics(publishingMode, _demoArea);
        var subscription =
            await client.SubscribeAsync<PublicStatisticRow>(publicStatisticsSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeThrottlingLimitsAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var throttlingLimitsSubscription = _subscribeRequestBuilder.CreateThrottlingLimits(publishingMode);
        var subscription =
            await client.SubscribeAsync<ThrottlingLimitMessage>(throttlingLimitsSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeCapacitiesAsync(IClient client, PublishingMode publishingMode,
        CancellationToken cancellationToken)
    {
        var contractsSubscription = _subscribeRequestBuilder.CreateCapacities(publishingMode, _demoArea);
        var subscription = await client.SubscribeAsync<CapacityRow>(contractsSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, client.ClientTarget, subscription, cancellationToken);
    }

    private async Task SendOrderRequestAsync(IClient client, CancellationToken cancellationToken)
    {
        var exampleData = GetExampleContractAndPortfolio(client);
        if (exampleData == default((ContractRow, Portfolio)))
        {
            return;
        }

        var orderRequest = new OrderEntryRequest()
        {
            RequestId = Guid.NewGuid().ToString(),
            RejectPartially = false,
            Orders =
            [
                new OrderEntry
                {
                    Text = "New order",
                    ClientOrderId = Guid.NewGuid().ToString(),
                    PortfolioId = exampleData.Portfolio!.Id,
                    Side = OrderSide.SELL,
                    ContractIds = [exampleData.Contract!.ContractId],
                    OrderType = OrderType.LIMIT,
                    Quantity = 3000,
                    State = OrderState.ACTI,
                    UnitPrice = 2500,
                    TimeInForce = TimeInForce.GFS,
                    DeliveryAreaId = exampleData.Portfolio.Areas.First().AreaId,
                    ExecutionRestriction = ExecutionRestriction.NON,
                    ExpireTime = DateTimeOffset.Now.AddHours(6),
                }
            ]
        };

        // Store created order in simple cache storage for order modification request
        _simpleCacheStorage.SetCache([orderRequest]);

        _logger.LogInformation("[{clientTarget}]Attempting to send correct order request.", client.ClientTarget);
        await client.SendAsync(orderRequest, DestinationHelper.ComposeDestination(_version,"orderEntryRequest"), cancellationToken);
    }

    private async Task SendOrderModificatonRequest(IClient client, CancellationToken cancellationToken)
    {
        // Get last created order for update purpose
        var lastOrder = _simpleCacheStorage.GetFromCache<OrderEntryRequest>()
            .LastOrDefault();
        if (lastOrder is null)
        {
            _logger.LogInformation("[{clientTarget}]No valid order to be used for order modification has been found!",
                client.ClientTarget);
        }
        var lastOrderEntry = lastOrder!.Orders.Single();

        // Get last order execution report response for above order request (OrderId required for order modification request)
        var lastOrderExecutionReport = _simpleCacheStorage.GetFromCache<OrderExecutionReport>()
            .FirstOrDefault(oer => oer.RequestId == lastOrder.RequestId);
        if (lastOrderExecutionReport is null || lastOrderExecutionReport.Orders.IsNullOrEmpty())
        {
            _logger.LogInformation("[{clientTarget}]No valid order execution report to be used for order modification has been found!",
                client.ClientTarget);
        }

        var lastOrderExecutionReportOrderEntry = lastOrderExecutionReport!.Orders.Single();

        var orderModificationRequest = new OrderModificationRequest()
        {
            RequestId = Guid.NewGuid().ToString(),
            OrderModificationType = OrderModificationType.DEAC,
            Orders =
            [
                new OrderModification()
                {
                    OrderId = lastOrderExecutionReportOrderEntry.OrderId,
                    RevisionNo = 0L,
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

        _logger.LogInformation("[{clientTarget}]Attempting to send an correct order modification request.",
            client.ClientTarget);
        await client.SendAsync(orderModificationRequest, DestinationHelper.ComposeDestination(_version,"orderModificationRequest"), cancellationToken);
    }

    private async Task SendInvalidOrderRequestAsync(IClient client,
        CancellationToken cancellationToken)
    {
        var exampleData = GetExampleContractAndPortfolio(client);
        if (exampleData == default)
        {
            return;
        }
        
        // Invalid order request - missing order details, detailed error will be logged as a part of order execution report response
        var invalidOrderRequest = new OrderEntryRequest()
        {
            RequestId = Guid.NewGuid().ToString(),
            RejectPartially = false,
            Orders =
            [
                new OrderEntry()
                {
                    ClientOrderId = Guid.NewGuid().ToString(),
                    PortfolioId = exampleData.Portfolio!.Id
                }
            ]
        };

        _logger.LogInformation("[{clientTarget}]Attempting to send incorrect order request.", client.ClientTarget);
        await client.SendAsync(invalidOrderRequest, DestinationHelper.ComposeDestination(_version,"orderEntryRequest"), cancellationToken);
    }

    private async Task SendInvalidOrderModificatonRequest(IClient client,
        CancellationToken cancellationToken)
    {
        var invalidOrderModificationRequest = new OrderModificationRequest()
        {
            RequestId = Guid.NewGuid().ToString(),
            OrderModificationType = OrderModificationType.DEAC,
            Orders =
            [
                new OrderModification()
                {
                    ClientOrderId = Guid.NewGuid().ToString()
                }
            ]
        };

        _logger.LogInformation("[{clientTarget}]Attempting to send an incorrect order modification request.",
            client.ClientTarget);
        await client.SendAsync(invalidOrderModificationRequest, DestinationHelper.ComposeDestination(_version,"orderModificationRequest"), cancellationToken);
    }

    private (ContractRow Contract, Portfolio Portfolio) GetExampleContractAndPortfolio(IClient client)
    {
        var exampleContract = _simpleCacheStorage
            .GetFromCache<ContractRow>()
            .FirstOrDefault(c => c.ProductType != ProductType.CUSTOM_BLOCK && c.DlvryAreaState.Any(s => s.State == ContractState.ACTI));
        if (exampleContract is null)
        {
            _logger.LogWarning(
                "[{clientTarget}]No valid contract to be used for order creation has been found! Check contracts available in {simpleCacheStorage} property.",
                client.ClientTarget, nameof(_simpleCacheStorage));
            return default;
        }

        var exampleAreas = exampleContract
            .DlvryAreaState
            .Where(s => s.State == ContractState.ACTI);
        
        var examplePortfolio = _simpleCacheStorage.GetFromCache<ConfigurationRow>()
            .FirstOrDefault()
            ?.Portfolios
            .FirstOrDefault(p => p.Areas.Any(a => exampleAreas.Any(s => s.DlvryAreaId == a.AreaId)));
        if (examplePortfolio is null)
        {
            _logger.LogWarning(
                "[{clientTarget}]No valid portfolio to be used for order creation has been found! Check contracts available in {simpleCacheStorage} property.",
                client.ClientTarget, nameof(_simpleCacheStorage));
            return default;
        }

        return (exampleContract, examplePortfolio);
    }
    
    private async Task ReadSubscriptionChannel<TValue>(IClient client, WebSocketClientTarget clientTarget,
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

                _simpleCacheStorage.SetCache(message.Data.ToList());
                var responseString = JsonConvert.SerializeObject(message);

                // Trimming response content
                responseString = responseString.Length > 250
                    ? responseString[..250] + "..."
                    : responseString;

                _logger.LogInformation(
                    "[{clientTarget}][Frame({SubscriptionId}):Response] : {ResponseString}", clientTarget,
                    subscription.Id, responseString);
            }
        }, cancellationToken);
    }
}