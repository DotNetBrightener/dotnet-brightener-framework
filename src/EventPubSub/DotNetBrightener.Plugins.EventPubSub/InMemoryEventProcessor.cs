using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace DotNetBrightener.Plugins.EventPubSub;

internal class InMemoryEventProcessor(IServiceScopeFactory serviceScopeFactory)
{
    public async Task ProcessEventMessage<T>(T eventMessage) where T : class, IEventMessage
    {
        var messageType   = eventMessage.GetType();
        var processorType = typeof(InMemoryEventProcessor<>).MakeGenericType(messageType);

        await using (var serviceScope = serviceScopeFactory.CreateAsyncScope())
        {
            var eventProcessor = serviceScope.ServiceProvider
                                             .GetRequiredService(processorType);

            var method = eventProcessor.GetMethodWithName(nameof(ProcessEventMessage), messageType);

            if (method is null)
                return;

            if (method.Invoke(eventProcessor,
                              new object[]
                              {
                                  eventMessage
                              }) is Task task)
            {
                await task;
            }
        }
    }
}

internal class InMemoryEventProcessor<T> where T : IEventMessage
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger              _logger;

    public InMemoryEventProcessor(ILoggerFactory                loggerFactory,
                                  IServiceScopeFactory          serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger              = loggerFactory.CreateLogger(GetType());
    }

    public async Task ProcessEventMessage(T eventMessage)
    {
        using (IServiceScope processingScope = _serviceScopeFactory.CreateScope())
        {
            var expectingEventHandlerType = typeof(IEventHandler<>).MakeGenericType(eventMessage.GetType());

            var allEventHandlers = processingScope!.ServiceProvider
                                                   .GetServices<IEventHandler>();

            ImmutableArray<IEventHandler> eventHandlers =
            [
                ..allEventHandlers
                 .Where(instance => instance.GetType()
                                            .IsAssignableTo(expectingEventHandlerType))
                 .OrderByDescending(x => x.Priority)
            ];

            if (eventMessage is INonStoppedEventMessage)
            {
                await eventHandlers.ParallelForEachAsync(eventHandler =>
                                                             HandleEventMessage(eventHandler, eventMessage));

                return;
            }

            foreach (var eventHandler in eventHandlers)
            {
                var shouldContinue = await HandleEventMessage(eventHandler, eventMessage);

                if (!shouldContinue)
                    break;
            }
        }
    }

    private async Task<bool> HandleEventMessage(IEventHandler x, T eventMessage)
    {
        try
        {
            var handleEventMethod = x.GetMethodWithName(nameof(IEventHandler<T>.HandleEvent),
                                                        eventMessage.GetType());

            if (handleEventMethod?.Invoke(x,
                [
                    eventMessage
                ]) is Task<bool> result)
            {
                return await result;
            }

            return true;
        }
        catch (NotImplementedException exception)
        {
            _logger.LogDebug(exception, "Event handler not implemented for {eventMessageType}", typeof(T).Name);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while executing event handler for {eventMessageType}", typeof(T).Name);

            throw;
        }
    }
}