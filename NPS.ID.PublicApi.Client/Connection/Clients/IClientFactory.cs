using NPS.ID.PublicApi.Client.Connection.Enums;
using NPS.ID.PublicApi.Client.Connection.Options;
using NPS.ID.PublicApi.Client.Security.Options;

namespace NPS.ID.PublicApi.Client.Connection.Clients;

public interface IClientFactory 
{
    Task<IClient> CreateAsync(WebSocketClientTarget clientTarget, string clientId, CredentialsOptions credentialsOptions, WebSocketOptions webSocketOptions, CancellationToken cancellationToken);
}