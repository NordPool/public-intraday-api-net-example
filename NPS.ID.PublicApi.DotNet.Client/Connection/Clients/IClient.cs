using NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions;
using NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions.Requests;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Clients;

public interface IClient
{
    Task<bool> OpenAsync(CancellationToken cancellationToken);

    Task<ISubscription<TValue>> SubscribeAsync<TValue>(SubscribeRequest request, CancellationToken cancellationToken);

    Task SendAsync<TRequest>(TRequest request, string destination, CancellationToken cancellationToken)
        where TRequest : class, new();

    Task UnsubscribeAsync(string subscriptionId, CancellationToken cancellationToken);

    ValueTask DisconnectAsync();
}