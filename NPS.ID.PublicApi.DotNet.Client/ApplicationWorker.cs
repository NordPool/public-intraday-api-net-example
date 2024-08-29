using Extend;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nordpool.ID.PublicApi.v1;
using Nordpool.ID.PublicApi.v1.Order;
using Nordpool.ID.PublicApi.v1.Order.Request;
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

        Console.CancelKeyPress += (sender, eArgs) =>
        {
            _cancellationTokenSource.Cancel();
            eArgs.Cancel = true;
            Environment.Exit(0);
        };
    }

    public async Task RunAsync(string[] args)
    {
        var edgeClient = await CreateClientAsync(WebSocketClientTarget.Edge, _cancellationTokenSource.Token);
        var middlewareClient =
            await CreateClientAsync(WebSocketClientTarget.Middleware, _cancellationTokenSource.Token);

        // Delivery areas
        await SubscribeDeliveryAreasAsync(edgeClient, WebSocketClientTarget.Edge, _cancellationTokenSource.Token);
        await SubscribeDeliveryAreasAsync(middlewareClient, WebSocketClientTarget.Middleware,
            _cancellationTokenSource.Token);

        // Contracts
        await SubscribeContractsAsync(edgeClient, WebSocketClientTarget.Edge, PublishingMode.Conflated,
            _cancellationTokenSource.Token);
        await SubscribeContractsAsync(middlewareClient, WebSocketClientTarget.Middleware, PublishingMode.Conflated,
            _cancellationTokenSource.Token);

        await SubscribeContractsAsync(edgeClient, WebSocketClientTarget.Edge, PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        await SubscribeContractsAsync(middlewareClient, WebSocketClientTarget.Middleware, PublishingMode.Streaming,
            _cancellationTokenSource.Token);

        // Configurations 
        await SubscribeConfigurationsAsync(edgeClient, WebSocketClientTarget.Edge, _cancellationTokenSource.Token);
        await SubscribeConfigurationsAsync(middlewareClient, WebSocketClientTarget.Middleware,
            _cancellationTokenSource.Token);

        // Capacities
        await SubscribeCapacitiesAsync(edgeClient, WebSocketClientTarget.Edge, PublishingMode.Conflated,
            _cancellationTokenSource.Token);
        await SubscribeCapacitiesAsync(middlewareClient, WebSocketClientTarget.Middleware, PublishingMode.Conflated,
            _cancellationTokenSource.Token);

        await SubscribeCapacitiesAsync(edgeClient, WebSocketClientTarget.Edge, PublishingMode.Streaming,
            _cancellationTokenSource.Token);
        await SubscribeCapacitiesAsync(middlewareClient, WebSocketClientTarget.Middleware, PublishingMode.Streaming,
            _cancellationTokenSource.Token);

        // Order 
        // We wait some time in hope to get some example contracts and configurations that are needed for preparing example order request
        Thread.Sleep(5000);
        await SendOrderRequestAsync(edgeClient, WebSocketClientTarget.Edge, _cancellationTokenSource.Token);
        await SendOrderRequestAsync(middlewareClient, WebSocketClientTarget.Middleware, _cancellationTokenSource.Token);

        _cancellationTokenSource.Token.WaitHandle.WaitOne();
    }

    private async Task<IClient> CreateClientAsync(WebSocketClientTarget clientTarget,
        CancellationToken cancellationToken)
    {
        return await
            _genericClientFactory.CreateAsync(_clientId, clientTarget, cancellationToken);
    }

    private async Task SubscribeDeliveryAreasAsync(IClient client, WebSocketClientTarget clientTarget,
        CancellationToken cancellationToken)
    {
        var deliveryAreasRequest = _subscribeRequestBuilder.CreateDeliveryAreas();
        var subscription = await client.SubscribeAsync<DeliveryAreaRow>(deliveryAreasRequest, cancellationToken);
        await ReadSubscriptionChannel(client, clientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeContractsAsync(IClient client, WebSocketClientTarget clientTarget,
        PublishingMode publishingMode, CancellationToken cancellationToken)
    {
        var contractsSubscription = _subscribeRequestBuilder.CreateContracts(publishingMode);
        var subscription = await client.SubscribeAsync<ContractRow>(contractsSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, clientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeConfigurationsAsync(IClient client, WebSocketClientTarget clientTarget,
        CancellationToken cancellationToken)
    {
        var configurationsSubscription = _subscribeRequestBuilder.CreateConfiguration();
        var subscription = await client.SubscribeAsync<ConfigurationRow>(configurationsSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, clientTarget, subscription, cancellationToken);
    }

    private async Task SubscribeCapacitiesAsync(IClient client, WebSocketClientTarget clientTarget,
        PublishingMode publishingMode, CancellationToken cancellationToken)
    {
        var contractsSubscription = _subscribeRequestBuilder.CreateCapacities(publishingMode, _demoArea);
        var subscription = await client.SubscribeAsync<CapacityRow>(contractsSubscription, cancellationToken);
        await ReadSubscriptionChannel(client, clientTarget, subscription, cancellationToken);
    }

    private async Task SendOrderRequestAsync(IClient client, WebSocketClientTarget clientTarget, CancellationToken cancellationToken)
    {
        var exampleContract = _simpleCacheStorage.GetFromCache<ContractRow>(clientTarget)
            .FirstOrDefault();
        if (exampleContract is null)
        {
            _logger.LogWarning(
                $"No valid contract to be used for order creation has been found! Check contracts available in {nameof(_simpleCacheStorage)} property.");
            return;
        }

        var examplePortfolio = _simpleCacheStorage.GetFromCache<ConfigurationRow>(clientTarget)
            .FirstOrDefault()
            ?.Portfolios
            .FirstOrDefault();
        if (examplePortfolio is null)
        {
            _logger.LogWarning(
                $"No valid portfolio to be used for order creation has been found! Check contracts available in {nameof(_simpleCacheStorage)} property.");
            return;
        }

        var orderEntryRequest = new OrderEntryRequest()
        {
            RequestId = Guid.NewGuid().ToString(),
            RejectPartially = false,
            Orders =
            [
                new OrderEntry
                {
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
        await client.SendAsync(orderEntryRequest, "orderEntryRequest", cancellationToken);
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
                var content = JsonConvert.SerializeObject(message);
                Console.WriteLine(
                    $"[{clientTarget}] Message read from the subscription's output channel. Type: '{subscription.Type}', destination: '{subscription.Destination}'.");
            }
        }, cancellationToken);
    }
}