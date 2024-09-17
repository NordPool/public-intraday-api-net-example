// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NPS.ID.PublicApi.DotNet.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddEndpointsOptions();
builder.Services.AddCredentialsOptions();
builder.Services.AddSsoClientOptions();

builder.Services.AddSsoClient();
builder.Services.AddStompClient();
builder.Services.AddGenericClientFactory();
builder.Services.AddWebSocketConnector();

builder.Services.AddApplicationWorker();

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var services = scope.ServiceProvider;

try
{
    await services
        .GetRequiredService<ApplicationWorker>()
        .RunAsync(args);
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}
