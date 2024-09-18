using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NPS.ID.PublicApi.Client.Connection;
using NPS.ID.PublicApi.Client.Connection.Clients;
using NPS.ID.PublicApi.Client.Connection.Options;
using NPS.ID.PublicApi.Client.Security;
using NPS.ID.PublicApi.Client.Security.Options;

namespace NPS.ID.PublicApi.Client;

public static class SsoClientServiceCollectionExtensions
{
    public static IServiceCollection AddEndpointsOptions(this IServiceCollection services)
    {
        services.AddOptions<EndpointsOptions>()
            .BindConfiguration(EndpointsOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
    
    public static IServiceCollection AddCredentialsOptions(this IServiceCollection services)
    {
        services.AddOptions<CredentialsOptions>()
            .BindConfiguration(CredentialsOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
    
    public static IServiceCollection AddSsoClientOptions(this IServiceCollection services)
    {
        services.AddOptions<SsoOptions>()
            .BindConfiguration(SsoOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddApplicationWorker(this IServiceCollection services)
    {
        return  services.AddSingleton<ApplicationWorker>();
    }
    
    public static IServiceCollection AddSsoClient(this IServiceCollection services)
    {
        services.AddTransient<ISsoService, SsoService>();

        services.AddHttpClient("SsoClient", (serviceProvider, client) =>
        {
            var ssoOptions = serviceProvider.GetRequiredService<IOptions<SsoOptions>>();
            client!.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ssoOptions.Value.ClientId}:{ssoOptions.Value.ClientSecret}")));

            client.BaseAddress = ssoOptions.Value.Uri;
        });

        return services;
    }

    public static IServiceCollection AddGenericClientFactory(this IServiceCollection services)
    {
        return services.AddSingleton<IGenericClientFactory, GenericClientFactory>();
    }
    
    public static IServiceCollection AddStompClient(this IServiceCollection services)
    {
        return services.AddTransient<IClientFactory, StompClientFactory>();
    }
    
    public static IServiceCollection AddWebSocketConnector(this IServiceCollection services)
    {
        return services.AddTransient<IWebSocketConnectorFactory, WebSocketConnectorFactory>();
    }
}