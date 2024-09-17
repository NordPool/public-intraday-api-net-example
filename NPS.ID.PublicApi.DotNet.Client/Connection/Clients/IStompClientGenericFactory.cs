using NPS.ID.PublicApi.DotNet.Client.Connection.Enums;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Clients;

public interface IGenericClientFactory
{
    Task<IClient> CreateAsync(string clientId, WebSocketClientTarget clientTarget, CancellationToken cancellationToken);
}