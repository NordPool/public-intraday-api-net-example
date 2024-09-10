using NPS.ID.PublicApi.DotNet.Client.Connection.Enums;
using NPS.ID.PublicApi.DotNet.Client.Connection.Options;
using NPS.ID.PublicApi.DotNet.Client.Security.Options;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Clients;

public interface IClientFactory 
{
    Task<IClient> CreateAsync(WebSocketClientTarget clientTarget, string clientId, CredentialsOptions credentialsOptions, WebSocketOptions webSocketOptions, CancellationToken cancellationToken);
}