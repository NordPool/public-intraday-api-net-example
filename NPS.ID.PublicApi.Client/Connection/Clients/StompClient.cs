using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using NPS.ID.PublicApi.Client.Connection.Enums;
using NPS.ID.PublicApi.Client.Connection.Events;
using NPS.ID.PublicApi.Client.Connection.Messages;
using NPS.ID.PublicApi.Client.Connection.Subscriptions;
using NPS.ID.PublicApi.Client.Connection.Subscriptions.Exceptions;
using NPS.ID.PublicApi.Client.Connection.Subscriptions.Requests;
using NPS.ID.PublicApi.Client.Connection.Extensions;
using NPS.ID.PublicApi.Client.Connection.Options;
using NPS.ID.PublicApi.Client.Security.Options;
using Stomp.Net.Stomp.Protocol;

namespace NPS.ID.PublicApi.Client.Connection.Clients;

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
    public WebSocketClientTarget ClientTarget { get; }
    public string ClientId { get; }

    public StompClient(
        ILogger<StompClient> logger,
        ILoggerFactory loggerFactory,
        WebSocketConnectorFactory webSocketConnectorFactory,
        WebSocketClientTarget target,
        string clientId,
        WebSocketOptions webSocketOptions,
        CredentialsOptions credentialsOptions)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _webSocketConnector = webSocketConnectorFactory
            .Create(webSocketOptions, credentialsOptions, OnMessageReceivedAsync, OnConnectionEstablishedAsync, OnConnectionClosedAsync, OnStompErrorAsync);
        ClientTarget = target;
        ClientId = clientId;
    }

    private Task OnConnectionEstablishedAsync()
    {
        _connectionEstablished = true;
        _connectionEstablishedEvent.Set();
        _logger.LogInformation("[{clientTarget}] Connection established for client {ClientId}", ClientTarget, ClientId);
        
        return Task.CompletedTask;
    }

    private Task OnConnectionClosedAsync()
    {
        _connectionClosedEvent.Set();

        if (_connectionClosed)
        {
            _logger.LogInformation("[{clientTarget}] Connection closing for client {ClientId}", ClientTarget, ClientId);
        }
        else
        {
            _logger.LogError("[{clientTarget}] Connection closed unexpectedly for client {ClientId}", ClientTarget, ClientId);
        }

        foreach (var subscription in _subscriptions.Values)
        {
            subscription.Close();
        }
        
        return Task.CompletedTask;
    }

    private Task OnMessageReceivedAsync(MessageReceivedEventArgs e, CancellationToken cancellationToken)
    {
        var isMessage = e.Message.IsMessageCommand();
        if (!isMessage)
        {
            return Task.CompletedTask;
        }
        
        var stompFrame = ConvertToStompFrame(e.Message);

        if (stompFrame.Properties.TryGetValue(Headers.Server.Subscription, out var subscriptionId))
        {
            _logger.LogInformation("[{clientTarget}][Frame({SubscriptionId}):Metadata] destination={Destination}, sentAt={SentAt}, snapshot={Snapshot}, publishingMode={PublishingMode}, sequenceNo={SequenceNo}", 
                ClientTarget, 
                subscriptionId,
                stompFrame.GetDestination(),
                stompFrame.GetSentAtTimestamp(),
                stompFrame.IsSnapshot(),
                stompFrame.GetPublishingMode(),
                stompFrame.GetSequenceNumber());

            if (_subscriptions.TryGetValue(subscriptionId, out var targetSubscription))
            {
                targetSubscription.OnMessage(stompFrame, e.Timestamp);
            }
            else
            {
                _logger.LogWarning("[{clientTarget}][Frame({SubscriptionId})] Received message for subscription that is not assigned to current client", ClientTarget, subscriptionId);
            }
        }
        else
        {
            if (stompFrame.Command == Commands.Server.Error && stompFrame.Properties.TryGetValue(Headers.Server.Message, out var errorMessage))
            {
                _logger.LogError("[{clientTarget}] Error message received from {StompConnectionUri}. Error: {Message}", ClientTarget, _webSocketConnector.ConnectionUri, errorMessage);
            }
            else
            {
                _logger.LogWarning("[{clientTarget}] Unrecognized message received from {StompConnectionUri}. Command:{Command}\nHeaders:\n{Headers}\n{Content}",
                    ClientTarget,
                    _webSocketConnector.ConnectionUri,
                    stompFrame.Command,
                    string.Join("\n", stompFrame.Properties.Select(header => $"{header.Key}:{header.Value}")),
                    Encoding.UTF8.GetString(stompFrame.Content));
            }
        }

        return Task.CompletedTask;
    }
    
    private Task OnStompErrorAsync(Exception exception)
    {
        _logger.LogWarning(exception, "[{clientTarget}] An error on web socket message processing", ClientTarget);

        return Task.CompletedTask;
    }
    
    private static StompFrame ConvertToStompFrame(ReceivedMessage message)
    {
        var messageStream = message.GetStream();
        //Remove the first char 'a' to get the json array
        messageStream.Seek(1, SeekOrigin.Begin);

        using var streamReader = new StreamReader(messageStream);
        var stompMessage = JsonSerializer.Deserialize<string[]>(streamReader.ReadToEnd()).ElementAt(0);
        
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
                $"[{ClientTarget}][Destination:{request.Destination}] Failed to subscribe because no connection is established! Connect first!");
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
        var payload = JsonSerializer.Serialize(request);
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

            _logger.LogInformation("[{clientTarget}][SubscriptionId:{Subscription}] Unsubscribed", ClientTarget, subscription.Id);
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        await UnsubscribeAllAsync(cancellationToken);
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