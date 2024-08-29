using Microsoft.Extensions.Options;
using NPS.ID.PublicApi.DotNet.Client.Connection.Enums;
using NPS.ID.PublicApi.DotNet.Client.Connection.Options;
using NPS.ID.PublicApi.DotNet.Client.Security.Options;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Clients;

public class GenericClientFactory : IGenericClientFactory
{
    private readonly CredentialsOptions _credentialsOptions;
    private readonly EndpointsOptions _endpointsOptions;
    private readonly IClientFactory _clientFactory;
    
    public GenericClientFactory(
        IOptions<CredentialsOptions> credentialsOptions,
        IOptions<EndpointsOptions> endpointOptions,
        IClientFactory clientFactory)
    {
        _credentialsOptions = credentialsOptions.Value;
        _endpointsOptions = endpointOptions.Value;
        _clientFactory = clientFactory;
    }

    public Task<IClient> CreateAsync(string clientId, WebSocketClientTarget target, CancellationToken cancellationToken)
    {
        var options = target switch
        {
            WebSocketClientTarget.Middleware => _endpointsOptions.Middleware,
            WebSocketClientTarget.Edge => _endpointsOptions.Edge,
            _ => throw new ArgumentOutOfRangeException(nameof(target))
        };

        return _clientFactory.CreateAsync(clientId, _credentialsOptions, options, cancellationToken);
    }
}