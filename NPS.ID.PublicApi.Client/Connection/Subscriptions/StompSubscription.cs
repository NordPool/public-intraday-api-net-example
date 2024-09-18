using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using NPS.ID.PublicApi.Client.Connection.Messages;
using NPS.ID.PublicApi.Client.Connection.Extensions;
using Stomp.Net.Stomp.Protocol;

namespace NPS.ID.PublicApi.Client.Connection.Subscriptions;

public class StompSubscription<TValue> : Subscription, ISubscription<TValue>
{
    private readonly JsonSerializerOptions _settings = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
    
    private readonly ILogger<StompSubscription<TValue>> _logger;
    private readonly Channel<ReceivedMessage<IReadOnlyCollection<TValue>>> _channel;

    public ChannelReader<ReceivedMessage<IReadOnlyCollection<TValue>>> OutputChannel => _channel.Reader;
    
    public StompSubscription(
        string id,
        string type,
        string destination,
        ILogger<StompSubscription<TValue>> logger,
        Channel<ReceivedMessage<IReadOnlyCollection<TValue>>> channel)
        : base(id,type,  destination)
    {
        _logger = logger;
        _channel = channel;
    }

    public override void OnMessage(StompFrame frame, DateTimeOffset timestamp)
    {
        var contentBytes = new ReadOnlySpan<byte>(frame.Content);
        
        try
        {
            var data = JsonSerializer.Deserialize<IReadOnlyCollection<TValue>>(contentBytes, _settings);
            if (data is null)
            {
                return;
            }

            var message = new ReceivedMessage<IReadOnlyCollection<TValue>>(data, timestamp, frame.IsSnapshot(), frame.GetPublishingMode());

            if (_channel.Writer.TryWrite(message))
            {
                return;
            }
            
            _logger.LogWarning("[SubscriptionId:{SubscriptionId}][Destination:{Destination}Channel for the subscription has reached its maximum capacity", Id, Destination);
            
            _channel.Writer.WriteAsync(message).AsTask()
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "An error on web socket message handling");
            _channel.Writer.Complete(e);
        }
    }

    public sealed override void Close()
    {
        _channel.Writer.Complete();
    }
}