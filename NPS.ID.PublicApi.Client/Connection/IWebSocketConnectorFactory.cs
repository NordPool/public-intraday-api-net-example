using NPS.ID.PublicApi.Client.Connection.Options;
using NPS.ID.PublicApi.Client.Security.Options;

namespace NPS.ID.PublicApi.Client.Connection;

public interface IWebSocketConnectorFactory
{
    WebSocketConnector Create(WebSocketOptions webSocketOptions, CredentialsOptions credentialsOptions);
}