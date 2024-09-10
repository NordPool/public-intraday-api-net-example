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

    public Task<IClient> CreateAsync(string clientId, WebSocketClientTarget clientTarget, CancellationToken cancellationToken)
    {
        var options = clientTarget switch
        {
            WebSocketClientTarget.Middleware => _endpointsOptions.Middleware,
            WebSocketClientTarget.PMD => _endpointsOptions.Pmd,
            _ => throw new ArgumentOutOfRangeException(nameof(clientTarget))
        };

        return _clientFactory.CreateAsync(clientTarget, clientId, _credentialsOptions, options, cancellationToken);
    }
}