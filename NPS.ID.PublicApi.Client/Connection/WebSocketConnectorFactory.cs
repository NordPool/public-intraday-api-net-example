using Microsoft.Extensions.Logging;
using NPS.ID.PublicApi.Client.Connection.Options;
using NPS.ID.PublicApi.Client.Security;
using NPS.ID.PublicApi.Client.Security.Options;

namespace NPS.ID.PublicApi.Client.Connection;

public class WebSocketConnectorFactory : IWebSocketConnectorFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISsoService _ssoService;

    public WebSocketConnectorFactory(
        ILoggerFactory loggerFactory,
        ISsoService ssoService)
    {
        _loggerFactory = loggerFactory;
        _ssoService = ssoService;
    }

    public WebSocketConnector Create(WebSocketOptions webSocketOptions, CredentialsOptions credentialsOptions)
    {
        return new WebSocketConnector(_loggerFactory.CreateLogger<WebSocketConnector>(), _ssoService, webSocketOptions, credentialsOptions);
    }
}