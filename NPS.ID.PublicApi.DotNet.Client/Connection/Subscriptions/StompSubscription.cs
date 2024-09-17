using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPS.ID.PublicApi.DotNet.Client.Connection.Extensions;
using NPS.ID.PublicApi.DotNet.Client.Connection.Messages;
using Stomp.Net.Stomp.Protocol;

namespace NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions;

public class StompSubscription<TValue> : Subscription, ISubscription<TValue>
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        NullValueHandling = NullValueHandling.Ignore
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
        using var memoryStream = new MemoryStream(frame.Content);
        using var reader = new StreamReader(memoryStream, Encoding.UTF8);
        using var jsonReader = new JsonTextReader(reader);
        
        try
        {
            var data = JsonSerializer.Create(Settings).Deserialize<IReadOnlyCollection<TValue>>(jsonReader);
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
            
            _channel.Writer.WriteAsync(message).AsTask().GetAwaiter().GetResult();
        }
        catch (Exception exception)
        {
            _channel.Writer.Complete(exception);
        }
    }

    public sealed override void Close()
    {
        _channel.Writer.Complete();
    }
}