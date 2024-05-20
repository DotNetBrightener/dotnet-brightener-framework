using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;

internal interface IAzureServiceBusEventSubscription
{
    Task<bool> ProcessMessageAsync(AzureServiceBusEventMessage message);
}

internal class AzureServiceBusEventSubscription<T>(
    IEnumerable<IEventHandler<T>> eventHandlers,
    ILoggerFactory                loggerFactory)
    : IAzureServiceBusEventSubscription
    where T : IDistributedEventMessage
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AzureServiceBusEventSubscription<T>>();

    public async Task<bool> ProcessMessageAsync(AzureServiceBusEventMessage message)
    {
        if (!eventHandlers.Any())
        {
            return false;
        }

        var eventMessage = message.GetPayload<T>();

        var orderedEventHandlers = eventHandlers.OrderByDescending(eventHandler => eventHandler.Priority);

        foreach (var handler in orderedEventHandlers)
        {
            _logger.LogInformation($"Processing message {eventMessage.GetType().GetTopicName()} using handler {handler.GetType().Name}");

            await handler.HandleEvent(eventMessage);
        }

        return true;
    }
}