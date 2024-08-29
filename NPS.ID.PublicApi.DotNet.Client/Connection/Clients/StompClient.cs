using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using NPS.ID.PublicApi.DotNet.Client.Connection.Extensions;
using NPS.ID.PublicApi.DotNet.Client.Connection.Events;
using NPS.ID.PublicApi.DotNet.Client.Connection.Messages;
using NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions;
using NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions.Exceptions;
using NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions.Requests;
using Stomp.Net.Stomp.Protocol;
using ServerCommands = NPS.ID.PublicApi.DotNet.Client.Connection.Messages.Commands.Server;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Clients;

public class StompClient : IClient
{
    private bool _connectionEstablished;
    private bool _connectionClosed;

    private readonly AsyncManualResetEvent _connectionEstablishedEvent = new();
    private readonly AsyncManualResetEvent _connectionClosedEvent = new();

    private readonly ILogger<StompClient> _logger;
    
    private readonly ILoggerFactory _loggerFactory;

    private readonly WebSocketConnector _webSocketConnector;

    private readonly Dictionary<string, Subscription> _subscriptions = new();
    public string ClientId { get; }

    public StompClient(
        ILogger<StompClient> logger,
        ILoggerFactory loggerFactory,
        WebSocketConnector webSocketConnector,
        string clientId)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _webSocketConnector = webSocketConnector;
        _webSocketConnector.StompConnectionEstablished += OnConnectionEstablished;
        _webSocketConnector.StompConnectionClosed += OnConnectionClosed;
        _webSocketConnector.MessageReceived += OnMessageReceivedAsync;
        ClientId = clientId;
    }

    private void OnConnectionEstablished(object sender, EventArgs e)
    {
        _connectionEstablished = true;
        _connectionEstablishedEvent.Set();
        _logger.LogInformation("Connection established for client {ClientId}", ClientId);
    }

    private void OnConnectionClosed(object sender, EventArgs e)
    {
        _connectionClosedEvent.Set();

        if (_connectionClosed)
        {
            _logger.LogInformation("Connection closing for client {ClientId}", ClientId);
        }
        else
        {
            _logger.LogError("Connection closed unexpectedly for client {ClientId}", ClientId);
        }

        foreach (var subscription in _subscriptions.Values)
        {
            subscription.Close();
        }
    }

    private Task OnMessageReceivedAsync(object sender, MessageReceivedEventArgs e, CancellationToken cancellationToken)
    {
        var isMessage = e.Message.IsMessageCommand();
        if (!isMessage)
        {
            return Task.CompletedTask;
        }
        
        var stompFrame = ConvertToStompFrame(e.Message);

        if (stompFrame.Properties.TryGetValue(Headers.Server.Subscription, out var subscriptionId))
        {
            if (_subscriptions.TryGetValue(subscriptionId, out var targetSubscription))
            {
                targetSubscription.OnMessage(stompFrame, e.Timestamp);
            }
            else
            {
                _logger.LogWarning("Received message for subscription {SubscriptionId} that is not assigned to current client", subscriptionId);
            }
        }
        else
        {
            if (stompFrame.Command == ServerCommands.Error && stompFrame.Properties.TryGetValue(Headers.Server.Message, out var errorMessage))
            {
                _logger.LogError("Error message received from {StompConnectionUri}. Error: {Message}", _webSocketConnector.ConnectionUri, errorMessage);
            }
            else
            {
                _logger.LogWarning("Unrecognized message received from {StompConnectionUri}. Command:{Command}\nHeaders:\n{Headers}\n{Content}",
                    _webSocketConnector.ConnectionUri,
                    stompFrame.Command,
                    string.Join("\n", stompFrame.Properties.Select(header => $"{header.Key}:{header.Value}")),
                    Encoding.UTF8.GetString(stompFrame.Content));
            }
        }

        return Task.CompletedTask;
    }
    
    private static StompFrame ConvertToStompFrame(ReceivedMessage message)
    {
        var messageStream = message.GetStream();
        //Remove the first char 'a' to get the json array
        messageStream.Seek(1, SeekOrigin.Begin);

        using var streamReader = new StreamReader(messageStream);
        var stompMessage = JsonConvert.DeserializeObject<string[]>(streamReader.ReadToEnd()).ElementAt(0);
        
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(stompMessage));
        using var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8);

        var frame = new StompFrame(true);
        frame.FromStream(binaryReader);

        return frame;
    }

    public async Task<bool> OpenAsync(CancellationToken cancellationToken)
    {
        await _webSocketConnector.ConnectAsync(cancellationToken);

        await _connectionEstablishedEvent.WaitAsync(cancellationToken);
        return _webSocketConnector.IsConnected;
    }

    public async Task<ISubscription<TValue>> SubscribeAsync<TValue>(SubscribeRequest request, CancellationToken cancellationToken)
    {
        if (!_webSocketConnector.IsConnected)
        {
            throw new SubscriptionFailedException(
                "Failed to subscribe because no connection is established! Connect first!");
        }

        var subscription = new StompSubscription<TValue>(
            request.SubscriptionId,
            request.Type,
            request.Destination,
            _loggerFactory.CreateLogger<StompSubscription<TValue>>(),
            Channel.CreateBounded<ReceivedMessage<IReadOnlyCollection<TValue>>>(new BoundedChannelOptions(30_000)
            {
                FullMode = BoundedChannelFullMode.Wait
            }));

        _subscriptions[subscription.Id] = subscription;
        var subscribeFrame = StompMessageFactory.SubscribeFrame(request.Destination, request.SubscriptionId);
        await _webSocketConnector.SendStompFrameAsync(subscribeFrame, cancellationToken);
        return subscription;
    }

    public async Task SendAsync<TRequest>(TRequest request, string destination, CancellationToken cancellationToken)
        where TRequest : class, new()
    {
        var payload = JsonConvert.SerializeObject(request);
        var payloadFrame = StompMessageFactory.SendFrame(payload, destination);
        await _webSocketConnector.SendStompFrameAsync(payloadFrame, cancellationToken);
    }

    public async Task UnsubscribeAsync(string subscriptionId, CancellationToken cancellationToken)
    {
        var unsubscribeFrame = StompMessageFactory.Unsubscribe(subscriptionId);
        await _webSocketConnector.SendStompFrameAsync(unsubscribeFrame, cancellationToken);

        if (_subscriptions.Remove(subscriptionId, out var subscription))
        {
            subscription.Close();

            _logger.LogInformation("Unsubscribed {Subscription}", subscription);
        }
    }

    public async ValueTask DisconnectAsync()
    {
        await UnsubscribeAllAsync(CancellationToken.None);
        _connectionClosed = true;
        await _webSocketConnector.DisposeAsync();
    }

    private async Task UnsubscribeAllAsync(CancellationToken cancellationToken)
    {
        if (!_webSocketConnector.IsConnected)
        {
            return;
        }

        var subscriptions = _subscriptions
            .Values
            .ToList();

        foreach (var subscription in subscriptions)
        {
            await UnsubscribeAsync(subscription.Id, cancellationToken);
        }
    }
}