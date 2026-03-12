using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub;

internal class InMemoryEventProcessorBackgroundJob(
    InMemoryEventMessageQueue                    eventMessageQueue,
    InMemoryEventProcessor                       eventProcessor,
    ILogger<InMemoryEventProcessorBackgroundJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var eventMessage in eventMessageQueue.ReadAllAsync(stoppingToken))
        {
            try
            {
                logger.LogDebug("Processing event message {messageType}", eventMessage.GetType().Name);

                await eventProcessor.ProcessEventMessage(eventMessage);

                logger.LogDebug("Processed event message {messageType}", eventMessage.GetType().Name);
            }
            catch (Exception exception)
            {
                logger.LogError(exception,
                                "Error while processing event message {messageType}",
                                eventMessage.GetType().Name);
            }
        }
    }
}