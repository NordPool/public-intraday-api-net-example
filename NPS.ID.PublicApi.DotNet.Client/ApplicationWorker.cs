using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nordpool.ID.PublicApi.v1;
using Nordpool.ID.PublicApi.v1.Order;
using Nordpool.ID.PublicApi.v1.Order.Request;
using Nordpool.ID.PublicApi.v1.Statistic;
using Nordpool.ID.PublicApi.v1.Throttling;
using NPS.ID.PublicApi.DotNet.Client.Connection.Clients;
using NPS.ID.PublicApi.DotNet.Client.Connection.Enums;
using NPS.ID.PublicApi.DotNet.Client.Connection.Storage;
using NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions;
using NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions.Requests;
using NPS.ID.PublicApi.DotNet.Client.Security.Options;

namespace NPS.ID.PublicApi.DotNet.Client;

public class ApplicationWorker
{
    private readonly ILogger<ApplicationWorker> _logger;

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
        _subscribeRequestBuilder = SubscribeRequestBuilder.CreateBuilder(credentialsOptions.Value.UserName, "v1");

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
        await SubscribeContractsAsync(middlewareClient, PublishingMode.Conflated,
            _cancellationTokenSource.Token);

        // Local views
        await SubscribeLocalViewsAsync(pmdClient, PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        await SubscribeLocalViewsAsync(middlewareClient, PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        
        // Private trades
        await SubscribePrivateTradesAsync(middlewareClient, PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        
        // Tickers
        await SubscribeTickersAsync(pmdClient, PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        await SubscribeTickersAsync(middlewareClient, PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        
        // MyTickers
        await SubscribeMyTickersAsync(pmdClient, PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        await SubscribeMyTickersAsync(middlewareClient, PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        
        // Public statistics
        await SubscribePublicStatisticsAsync(pmdClient, PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        await SubscribePublicStatisticsAsync(middlewareClient,
            PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        
        // Throttling limits
        await SubscribeThrottlingLimitsAsync(middlewareClient,
            PublishingMode.Conflated,
            _cancellationTokenSource.Token);
        
        // Capacities
        await SubscribeCapacitiesAsync(pmdClient, PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        await SubscribeCapacitiesAsync(middlewareClient, PublishingMode.Streaming,
            _cancellationTokenSource.Token);

        // Order 
        // We wait some time in hope to get some example contracts and configurations that are needed for preparing example order request
        Thread.Sleep(5000);
        await SendOrderRequestAsync(middlewareClient, _cancellationTokenSource.Token);
        await SendOrderModificatonRequest(middlewareClient,
            _cancellationTokenSource.Token);
        await SendInvalidOrderRequestAsync(middlewareClient,
            _cancellationTokenSource.Token);
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
        var publicStatisticsSubscription = _subscribeRequestBuilder.CreatePublicStatistics(publishingMode);
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
        var exampleContract = _simpleCacheStorage.GetFromCache<ContractRow>(client.ClientTarget)
            .FirstOrDefault();
        if (exampleContract is null)
        {
            _logger.LogWarning(
                "[{clientTarget}]No valid contract to be used for order creation has been found! Check contracts available in {simpleCacheStorage} property.",
                client.ClientTarget, nameof(_simpleCacheStorage));
            return;
        }

        var examplePortfolio = _simpleCacheStorage.GetFromCache<ConfigurationRow>(client.ClientTarget)
            .FirstOrDefault()
            ?.Portfolios
            .FirstOrDefault();
        if (examplePortfolio is null)
        {
            _logger.LogWarning(
                "[{clientTarget}]No valid portfolio to be used for order creation has been found! Check contracts available in {simpleCacheStorage} property.",
                client.ClientTarget, nameof(_simpleCacheStorage));
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
                    PortfolioId = examplePortfolio!.Id,
                    Side = OrderSide.SELL,
                    ContractIds = [exampleContract!.ContractId],
                    OrderType = OrderType.LIMIT,
                    Quantity = 3000,
                    State = OrderState.ACTI,
                    UnitPrice = 2500,
                    TimeInForce = TimeInForce.GFS,
                    DeliveryAreaId = examplePortfolio.Areas.First().AreaId,
                    ExecutionRestriction = ExecutionRestriction.NON,
                    ExpireTime = DateTimeOffset.Now.AddHours(6)
                }
            ]
        };

        // Store created order in simple cache storage for order modification request
        _simpleCacheStorage.SetCache(client.ClientTarget, [orderRequest]);

        _logger.LogInformation("[{clientTarget}]Attempting to send correct order request.", client.ClientTarget);
        await client.SendAsync(orderRequest, "orderEntryRequest", cancellationToken);
    }

    private async Task SendOrderModificatonRequest(IClient client, CancellationToken cancellationToken)
    {
        // Get last created order for update purpose
        var lastOrder = _simpleCacheStorage.GetFromCache<OrderEntryRequest>(client.ClientTarget)
            .LastOrDefault();
        if (lastOrder is null)
        {
            _logger.LogInformation("[{clientTarget}]No valid order to be used for order modification has been found!",
                client.ClientTarget);
        }

        var lastOrderEntry = lastOrder!.Orders.First();

        var orderModificatonRequest = new OrderModificationRequest()
        {
            RequestId = Guid.NewGuid().ToString(),
            OrderModificationType = OrderModificationType.DEAC,
            Orders =
            [
                new OrderModification()
                {
                    OrderId = string.Empty,
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
        await client.SendAsync(orderModificatonRequest, "orderModificationRequest", cancellationToken);
    }

    private async Task SendInvalidOrderRequestAsync(IClient client,
        CancellationToken cancellationToken)
    {
        
        var invalidOrderRequest = new OrderEntryRequest()
        {
            RequestId = Guid.NewGuid().ToString(),
            RejectPartially = false,
            Orders =
            [
                new OrderEntry()
            ]
        };

        _logger.LogInformation("[{clientTarget}]Attempting to send incorrect order request.", client.ClientTarget);
        await client.SendAsync(invalidOrderRequest, "orderEntryRequest", cancellationToken);
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
            ]
        };

        _logger.LogInformation("[{clientTarget}]Attempting to send an incorrect order modification request.",
            client.ClientTarget);
        await client.SendAsync(invalidOrderModificationRequest, "orderModificationRequest", cancellationToken);
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

                _simpleCacheStorage.SetCache(clientTarget, message.Data.ToList());
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