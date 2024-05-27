using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace DotNetBrightener.Plugins.EventPubSub;

public class DefaultEventPublisher : IEventPublisher
{
    private readonly ConcurrentDictionary<object, Timer> _queue = new();
    private readonly ILogger                             _logger;
    private readonly IServiceScopeFactory                _serviceScopeFactory;

    public DefaultEventPublisher(IServiceScopeFactory serviceScopeFactory,
                                 ILoggerFactory       loggerFactory)
    {
        _logger              = loggerFactory.CreateLogger(GetType());
        _serviceScopeFactory = serviceScopeFactory;
    }

    public virtual Task Publish<T>(T eventMessage, bool runInBackground = false) where T : class, IEventMessage
    {
        if (eventMessage != null)
        {
            // Enable event throttling by allowing the very same event to be published only all 150 ms.
            if (_queue.TryGetValue(eventMessage, out _))
            {
                // do nothing. The same event was published a tick ago.
                return Task.CompletedTask;
            }

            _queue[eventMessage] = new Timer(RemoveFromQueue, eventMessage, 150, Timeout.Infinite);
        }

        if (!runInBackground)
            return PublishEvent(eventMessage);

        return Task.Run(async () => await PublishEvent(eventMessage));
    }

    private async Task PublishEvent<T>(T eventMessage) where T : class, IEventMessage
    {
        Type[] eventHandlersTypes;

        await using (var serviceScope = _serviceScopeFactory.CreateAsyncScope())
        {
            var t       = typeof(IEventHandler<>);
            var evtType = eventMessage.GetType();

            var eventHandlerType = t.MakeGenericType(evtType);

            var eventHandlers = serviceScope.ServiceProvider
                                            .GetServices(eventHandlerType)
                                            .OfType<IEventHandler>()
                                            .OrderByDescending(handler => handler.Priority)
                                            .ToArray();
            if (!eventHandlers.Any())
            {
                eventHandlers = serviceScope.ServiceProvider
                                            .GetServices<IEventHandler<T>>()
                                            .OrderByDescending(handler => handler.Priority)
                                            .ToArray();
            }

            if (eventMessage is INonStoppedEventMessage)
            {
                await Task.WhenAll(eventHandlers.Select(eventHandler => PublishEvent(eventHandler, eventMessage)));

                return;
            }

            eventHandlersTypes = eventHandlers.Select(eventHandler => eventHandler.GetType())
                                              .ToArray();
        }

        if (!eventHandlersTypes.Any())
            return;

        await using (var serviceScope = _serviceScopeFactory.CreateAsyncScope())
        {
            foreach (var eventHandlerType in eventHandlersTypes)
            {
                if (serviceScope.ServiceProvider.GetService(eventHandlerType) is not IEventHandler eventHandler)
                    continue;

                var shouldContinue = await PublishEvent(eventHandler, eventMessage);

                if (!shouldContinue)
                    break;
            }
        }
    }

    private async Task<bool> PublishEvent<T>(IEventHandler x, T eventMessage) where T : class, IEventMessage
    {
        try
        {
            var handleEventMethod = x.GetMethodWithName("HandleEvent", eventMessage.GetType());

            return await (((Task<bool>)handleEventMethod.Invoke(x,
                                                                new object[]
                                                                {
                                                                    eventMessage
                                                                }))!);
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

    private void RemoveFromQueue(object eventMessage)
    {
        if (_queue.TryRemove(eventMessage, out var timer))
        {
            timer.Dispose();
        }
    }
}