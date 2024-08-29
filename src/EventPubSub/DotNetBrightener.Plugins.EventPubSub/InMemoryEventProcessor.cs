using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger            _logger;
    private readonly IEventHandler<T>[] _eventHandlers;

    public InMemoryEventProcessor(ILoggerFactory                loggerFactory,
                                  IEnumerable<IEventHandler<T>> eventHandlers)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _eventHandlers = eventHandlers.OrderByDescending(handler => handler.Priority)
                                      .ToArray();
    }

    public async Task ProcessEventMessage(T eventMessage)
    {
        if (eventMessage is INonStoppedEventMessage)
        {
            await _eventHandlers.ParallelForEachAsync(eventHandler => HandleEventMessage(eventHandler, eventMessage));

            return;
        }

        foreach (var eventHandler in _eventHandlers)
        {
            var shouldContinue = await HandleEventMessage(eventHandler, eventMessage);

            if (!shouldContinue)
                break;
        }
    }

    private async Task<bool> HandleEventMessage(IEventHandler<T> x, T eventMessage)
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