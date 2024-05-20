using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub;

public class DefaultEventPublisher : IEventPublisher
{
    private readonly ConcurrentDictionary<object, Timer> _queue = new();
    private readonly ILogger                             _logger;
    private readonly IServiceScopeFactory                _serviceResolver;

    public DefaultEventPublisher(IServiceScopeFactory serviceResolver,
                                 ILoggerFactory       loggerFactory)
    {
        _logger          = loggerFactory.CreateLogger(GetType());
        _serviceResolver = serviceResolver;
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
        Type[] eventHandlersTypes = Array.Empty<Type>();

        await using (var serviceScope = _serviceResolver.CreateAsyncScope())
        {
            var eventHandlers = serviceScope.ServiceProvider
                                            .GetServices<IEventHandler<T>>()
                                            .OrderByDescending(_ => _.Priority)
                                            .ToArray();

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

        await using (var serviceScope = _serviceResolver.CreateAsyncScope())
        {
            foreach (var eventHandlerType in eventHandlersTypes)
            {
                if (serviceScope.ServiceProvider.GetService(eventHandlerType) is not IEventHandler<T> eventHandler)
                    continue;

                var shouldContinue = await PublishEvent(eventHandler, eventMessage);

                if (!shouldContinue)
                    break;
            }
        }
    }

    private async Task<bool> PublishEvent<T>(IEventHandler<T> x, T eventMessage) where T : class, IEventMessage
    {
        try
        {
            return await x.HandleEvent(eventMessage);
        }
        catch (NotImplementedException exception)
        {
            _logger.LogWarning(exception, "Event handler not implemented for {eventMessageType}", typeof(T).Name);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while executing event handler for {eventMessageType}", typeof(T).Name);

            return false;
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