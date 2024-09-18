using NPS.ID.PublicApi.Client.Connection.Enums;

namespace NPS.ID.PublicApi.Client.Connection.Clients;

public interface IGenericClientFactory
{
    Task<IClient> CreateAsync(string clientId, WebSocketClientTarget clientTarget, CancellationToken cancellationToken);
}