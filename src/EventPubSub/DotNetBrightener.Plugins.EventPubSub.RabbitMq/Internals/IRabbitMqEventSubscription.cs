using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DotNetBrightener.Plugins.EventPubSub.RabbitMq.Internals;

internal interface IRabbitMqEventSubscription
{
    Task<bool> ProcessMessage(EventMessageWrapper messageWrapper);
}

internal class RabbitMqEventSubscription<T>(
    IEnumerable<IEventHandler<T>> eventHandlers,
    ILoggerFactory                loggerFactory)
    : IRabbitMqEventSubscription
    where T : DistributedEventMessage
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<RabbitMqEventSubscription<T>>();

    public async Task<bool> ProcessMessage(EventMessageWrapper messageWrapper)
    {
        if (!eventHandlers.Any())
            return false;

        var eventMessage = ExtractPayload<T>(messageWrapper);

        if (eventMessage == null)
        {
            _logger.LogInformation("No message body to process for exchange {exchangeName}.",
                                   typeof(T).GetExchangeName());

            return false;
        }

        if (string.IsNullOrWhiteSpace(eventMessage.FromApp))
            eventMessage.FromApp = messageWrapper.CurrentApp;

        if (string.IsNullOrWhiteSpace(eventMessage.OriginApp))
            eventMessage.OriginApp = messageWrapper.OriginApp;

        var orderedEventHandlers = eventHandlers.OrderByDescending(eventHandler => eventHandler.Priority)
                                                .ToArray();

        foreach (var handler in orderedEventHandlers)
        {
            // set origin payload via reflection since OriginPayload may be read-only
            var originProp = handler.GetType().GetProperty("OriginPayload");
            originProp?.SetValue(handler, messageWrapper);

            _logger.LogInformation("Processing message {exchangeName} using handler {handlerName}",
                                   eventMessage.GetType().GetExchangeName(),
                                   handler.GetType().Name);

            await handler.HandleEvent(eventMessage);
        }

        return true;
    }

    /// <summary>
    ///     Extracts the typed payload from the message wrapper using JSON deserialization.
    /// </summary>
    private static TPayload ExtractPayload<TPayload>(EventMessageWrapper wrapper) where TPayload : class
    {
        var typeName = typeof(T).FullName;

        if (wrapper.Payload is not null &&
            wrapper.Payload.TryGetValue(typeName, out var payloadObj) &&
            payloadObj is string payloadJson)
        {
            return JsonConvert.DeserializeObject<TPayload>(payloadJson);
        }

        return null;
    }
}
