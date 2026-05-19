using Microsoft.Extensions.Configuration;
using NPS.ID.PublicApi.BinaryApiClient;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.local.json", optional: true)
    .Build();

var settings = config.GetSection("BinaryApi").Get<BinaryApiSettings>()
    ?? throw new InvalidOperationException("Missing 'BinaryApi' section in appsettings.json");

if (string.IsNullOrWhiteSpace(settings.Host))
    throw new InvalidOperationException("BinaryApi:Host must be set in appsettings.json");

if (string.IsNullOrWhiteSpace(settings.Username) || string.IsNullOrWhiteSpace(settings.Password))
    throw new InvalidOperationException("BinaryApi:Username and BinaryApi:Password must be set in appsettings.json or appsettings.local.json");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // prevent immediate termination
    cts.Cancel();
};

Console.WriteLine("Binary API Test Client");
Console.WriteLine($"Host: {settings.Host}");
Console.WriteLine("Press Ctrl+C to exit.");
Console.WriteLine();

var ssoClient = new SsoClient(settings);
var client = new BinaryApiClient(settings, ssoClient);
await client.RunAsync(cts.Token);