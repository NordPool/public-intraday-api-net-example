using Microsoft.Extensions.Logging;
using NPS.ID.PublicApi.DotNet.Client.Connection.Enums;
using NPS.ID.PublicApi.DotNet.Client.Connection.Options;
using NPS.ID.PublicApi.DotNet.Client.Security.Options;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Clients;

public class StompClientFactory : IClientFactory
{
    private readonly IWebSocketConnectorFactory _webSocketConnectorFactory;
    private readonly ILoggerFactory _loggerFactory;

    private readonly TimeSpan _connectionAttemptTimeout = TimeSpan.FromSeconds(600);

    public StompClientFactory(ILoggerFactory loggerFactory, IWebSocketConnectorFactory webSocketConnectorFactory)
    {
        _loggerFactory = loggerFactory;
        _webSocketConnectorFactory = webSocketConnectorFactory;
    }

    public async Task<IClient> CreateAsync(WebSocketClientTarget clientTarget, string clientId, CredentialsOptions credentialsOptions, WebSocketOptions connectionOptions, CancellationToken cancellationToken)
    {
        var webSocketConnector = _webSocketConnectorFactory.Create(connectionOptions, credentialsOptions);

        IClient client = new StompClient(
            _loggerFactory.CreateLogger<StompClient>(),
            _loggerFactory,
            webSocketConnector,
            clientTarget,
            clientId);

        var timeoutCancellationToken = new CancellationTokenSource(_connectionAttemptTimeout).Token;
        var connectedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellationToken, cancellationToken).Token;

        await client.OpenAsync(connectedCancellationToken);

        return client;
    }
}