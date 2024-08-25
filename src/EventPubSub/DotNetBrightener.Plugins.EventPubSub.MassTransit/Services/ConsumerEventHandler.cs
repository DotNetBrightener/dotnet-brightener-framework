using MassTransit;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub.MassTransit.Services;

internal class ConsumerEventHandler<TEventMessage> : IConsumer<TEventMessage>
    where TEventMessage : DistributedEventMessage, new()
{
    private readonly List<DistributedEventHandler<TEventMessage>> _distributedEventEventHandlers;
    private readonly ILogger<ConsumerEventHandler<TEventMessage>> _logger;

    public ConsumerEventHandler(IEnumerable<IEventHandler<TEventMessage>>    distributedEventEventHandlers,
                                ILogger<ConsumerEventHandler<TEventMessage>> logger)
    {
        _logger = logger;
        _distributedEventEventHandlers = distributedEventEventHandlers.OfType<DistributedEventHandler<TEventMessage>>()
                                                                      .OrderByDescending(x => x.Priority)
                                                                      .ToList();
    }

    public async Task Consume(ConsumeContext<TEventMessage> context)
    {
        if (!_distributedEventEventHandlers.Any())
            return;

        var eventMessage = context.Message;

        await _distributedEventEventHandlers.ParallelForEachAsync(async handler =>
        {
            handler.OriginPayload = new MassTransitEventMessageWrapper
            {
                CorrelationId = eventMessage.CorrelationId,
                CreatedOn     = eventMessage.CreatedOn,
                MachineName   = context.SourceAddress?.Host,
            };

            await handler.HandleEvent(eventMessage);
        });

        _logger.LogInformation("CorrelationId: {CorrelationId}. " +
                               "Context CorrelationId: {contextCorrelationId}. " +
                               "Created on: {@CreatedOn}",
                               eventMessage.CorrelationId,
                               context.CorrelationId,
                               eventMessage.CreatedOn);
    }
}