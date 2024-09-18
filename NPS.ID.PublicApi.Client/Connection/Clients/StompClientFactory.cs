using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPS.ID.PublicApi.Client.Connection.Enums;
using NPS.ID.PublicApi.Client.Connection.Options;
using NPS.ID.PublicApi.Client.Security.Options;

namespace NPS.ID.PublicApi.Client.Connection.Clients;

public class StompClientFactory
{
    private readonly ILoggerFactory _loggerFactory;

    private readonly TimeSpan _connectionAttemptTimeout = TimeSpan.FromSeconds(60);

    private readonly CredentialsOptions _credentialsOptions;
    private readonly EndpointsOptions _endpointsOptions;
    
    private readonly WebSocketConnectorFactory _webSocketConnectorFactory;
    
    public StompClientFactory(ILoggerFactory loggerFactory,
        IOptions<CredentialsOptions> credentialsOptions,
        IOptions<EndpointsOptions> endpointOptions,
        WebSocketConnectorFactory webSocketConnectorFactory)
    {
        _loggerFactory = loggerFactory;
        _credentialsOptions = credentialsOptions.Value;
        _endpointsOptions = endpointOptions.Value;
        _webSocketConnectorFactory = webSocketConnectorFactory;
    }

    public async Task<IClient> CreateAsync(WebSocketClientTarget clientTarget, string clientId, CancellationToken cancellationToken)
    {
        var webSocketOptionsForClient = clientTarget switch
        {
            WebSocketClientTarget.TRADING => _endpointsOptions.Trading,
            WebSocketClientTarget.MARKET_DATA => _endpointsOptions.MarketData,
            _ => throw new ArgumentOutOfRangeException(nameof(clientTarget))
        };
        
        var client = new StompClient(
            _loggerFactory.CreateLogger<StompClient>(),
            _loggerFactory,
            _webSocketConnectorFactory,
            clientTarget,
            clientId,
            webSocketOptionsForClient,
            _credentialsOptions);

        var timeoutCancellationToken = new CancellationTokenSource(_connectionAttemptTimeout).Token;
        var connectedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellationToken, cancellationToken).Token;

        await client.OpenAsync(connectedCancellationToken);

        return client;
    }
}