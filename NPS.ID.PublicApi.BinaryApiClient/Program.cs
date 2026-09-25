using Microsoft.Extensions.Configuration;
using NPS.ID.PublicApi.BinaryApiClient;
using NPS.ID.PublicApi.BinaryApiClient.Cli;
using NPS.ID.PublicApi.BinaryApiClient.Pmd;
using NPS.ID.PublicApi.BinaryApiClient.Trading;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.local.json", optional: true)
    .Build();

var settings = config.GetSection("BinaryApi").Get<BinaryApiSettings>()
    ?? throw new InvalidOperationException("Missing 'BinaryApi' section in appsettings.json");

if (string.IsNullOrWhiteSpace(settings.Pmd.Host))
    throw new InvalidOperationException("BinaryApi:Pmd:Host must be set in appsettings.json");

if (string.IsNullOrWhiteSpace(settings.Trading.Host))
    throw new InvalidOperationException("BinaryApi:Trading:Host must be set in appsettings.json");

if (string.IsNullOrWhiteSpace(settings.Username) || string.IsNullOrWhiteSpace(settings.Password))
    throw new InvalidOperationException("BinaryApi:Username and BinaryApi:Password must be set in appsettings.json or appsettings.local.json");

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // prevent immediate termination
    cts.Cancel();
};

Console.WriteLine("Binary API Test Client");
Console.WriteLine($"PMD host:     {settings.Pmd.Host}");
Console.WriteLine($"Trading host: {settings.Trading.Host}  (autoHibe={settings.Trading.AutoHibe}, abortExisting={settings.Trading.AbortExisting})");
Console.WriteLine("Press Ctrl+C or type 'quit' to exit.");
Console.WriteLine();

var ssoClient = new SsoClient(settings);
var pmd = new PmdClient(settings, ssoClient);
var trading = new TradingClient(settings, ssoClient);
var commands = new CommandLoop(pmd, trading);

// Both connections reconnect independently; the console loop drives them. Whichever finishes first
// (normally the console loop on 'quit') shuts the rest down.
var pmdTask = pmd.RunAsync(cts.Token);
var tradingTask = trading.RunAsync(cts.Token);
var commandTask = commands.RunAsync(cts.Token);

await Task.WhenAny(pmdTask, tradingTask, commandTask);
cts.Cancel();

try
{
    await Task.WhenAll(pmdTask, tradingTask, commandTask);
}
catch (OperationCanceledException)
{
    // expected on shutdown
}

Console.WriteLine("Bye.");
