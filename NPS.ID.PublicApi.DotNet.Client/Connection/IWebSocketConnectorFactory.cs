using NPS.ID.PublicApi.DotNet.Client.Connection.Options;
using NPS.ID.PublicApi.DotNet.Client.Security.Options;

namespace NPS.ID.PublicApi.DotNet.Client.Connection;

public interface IWebSocketConnectorFactory
{
    WebSocketConnector Create(WebSocketOptions webSocketOptions, CredentialsOptions credentialsOptions);
}