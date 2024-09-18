using NPS.ID.PublicApi.Client.Connection.Enums;
using NPS.ID.PublicApi.Client.Connection.Subscriptions;
using NPS.ID.PublicApi.Client.Connection.Subscriptions.Requests;

namespace NPS.ID.PublicApi.Client.Connection.Clients;

public interface IClient
{
    WebSocketClientTarget ClientTarget { get; }
    
    Task<bool> OpenAsync(CancellationToken cancellationToken);

    Task<ISubscription<TValue>> SubscribeAsync<TValue>(SubscribeRequest request, CancellationToken cancellationToken);

    Task SendAsync<TRequest>(TRequest request, string destination, CancellationToken cancellationToken)
        where TRequest : class, new();

    Task UnsubscribeAsync(string subscriptionId, CancellationToken cancellationToken);

    Task DisconnectAsync(CancellationToken cancellationToken);
}