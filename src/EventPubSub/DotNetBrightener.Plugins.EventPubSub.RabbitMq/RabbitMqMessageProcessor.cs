using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DotNetBrightener.Plugins.EventPubSub.RabbitMq;

/// <summary>
///     Abstract base class for RabbitMQ message processing.
///     Handles serialization/deserialization of event messages to/from RabbitMQ.
/// </summary>
public abstract class RabbitMqMessageProcessor<TEventMessage>(IServiceProvider serviceProvider)
    : IRabbitMqMessageProcessor
    where TEventMessage : EventMessageWrapper, new()
{
    /// <inheritdoc />
    public virtual async Task<EventMessageWrapper> PrepareOutgoingMessage<T>(T                    message,
                                                                             EventMessageWrapper? originMessage = null)
        where T : IDistributedEventMessage
    {
        var eventMessage = (TEventMessage)ActivatorUtilities.CreateInstance(serviceProvider, typeof(TEventMessage));

        // serialize the message to JSON, then embed it as payload
        var payloadJson = JsonConvert.SerializeObject(message);

        eventMessage.Payload = new Dictionary<string, object?>
        {
            {
                typeof(T).FullName, payloadJson
            }
        };

        eventMessage.CorrelationId = originMessage?.CorrelationId ?? Guid.CreateVersion7();
        eventMessage.EventId       = Guid.CreateVersion7();
        eventMessage.CreatedOn     = DateTime.UtcNow;
        eventMessage.MachineName   = Environment.MachineName;
        eventMessage.OriginApp     = originMessage?.OriginApp;
        eventMessage.CurrentApp    = originMessage?.CurrentApp;

        if (originMessage is not null)
        {
            eventMessage.CorrelationId = originMessage.CorrelationId;
            eventMessage.OriginApp     = originMessage.OriginApp;
        }

        return eventMessage;
    }

    public virtual async Task<EventMessageWrapper> ParseIncomingMessage(string incomingJson)
    {
        var message = JsonConvert.DeserializeObject<TEventMessage>(incomingJson);

        if (message is null)
            throw new InvalidOperationException($"Failed to deserialize message to {typeof(TEventMessage).Name}");

        return message;
    }
}
