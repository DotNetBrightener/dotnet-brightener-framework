using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Internals;

internal interface IAzureServiceBusEventSubscription
{
    Task<bool> ProcessMessage(EventMessageWrapper messageWrapper, ProcessMessageEventArgs args);
}

internal class AzureServiceBusEventSubscription<T>(
    IEnumerable<IEventHandler<T>> eventHandlers,
    ILoggerFactory loggerFactory)
    : IAzureServiceBusEventSubscription
    where T : DistributedEventMessage
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AzureServiceBusEventSubscription<T>>();

    public async Task<bool> ProcessMessage(EventMessageWrapper messageWrapper, 
                                           ProcessMessageEventArgs args)
    {
        if (!eventHandlers.Any())
        {
            return false;
        }

        var eventMessage = messageWrapper.GetPayload<T>();

        if (eventMessage == null)
        {

            _logger.LogInformation("No message body to process for topic {topicName}.",
                                   typeof(T).GetTopicName());
            return false;
        }

        if (string.IsNullOrWhiteSpace(eventMessage.FromApp))
        {
            eventMessage.FromApp = messageWrapper.CurrentApp;
        }

        if (string.IsNullOrWhiteSpace(eventMessage.OriginApp))
        {
            eventMessage.OriginApp = messageWrapper.OriginApp;
        }

        var orderedEventHandlers = eventHandlers.OrderByDescending(eventHandler => eventHandler.Priority)
                                                .ToArray();

        foreach (var handler in orderedEventHandlers)
        {
            if (handler is DistributedEventHandler<T> distributedEventHandler)
            {
                distributedEventHandler.OriginPayload = messageWrapper;
            }

            _logger.LogInformation("Processing message {topicName} using handler {handlerName}",
                                   eventMessage.GetType().GetTopicName(),
                                   handler.GetType().Name);

            await handler.HandleEvent(eventMessage);
        }

        return true;
    }
}