using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetBrightener.Plugins.EventPubSub.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub
{
    public interface IEventPublisher
    {
        /// <summary>
        ///     Fires an event of type <typeparamref name="T"/>, optionally specify if it should be executed in background
        /// </summary>
        /// <typeparam name="T">Type of the event message</typeparam>
        /// <param name="eventMessage">The event message</param>
        /// <param name="runInBackground">
        ///     If <c>true</c>, another thread will be used to execute the task.
        ///     The thread will not share the resources objects from the calling scope so be cautioned using this property as <c>true</c>
        /// </param>
        Task Publish<T>(T eventMessage, bool runInBackground = false) where T : class, IEventMessage;
    }

    public class EventPublisher : IEventPublisher
    {
        private readonly ConcurrentDictionary<object, Timer> _queue = new ConcurrentDictionary<object, Timer>();
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceResolver;
        private readonly IServiceProvider _backgroundServiceResolver;

        public EventPublisher(IServiceProvider serviceResolver,
                              IBackgroundServiceProvider backgroundServiceResolver,
                              ILogger<EventPublisher> logger)
        {
            _logger = logger;
            _serviceResolver = serviceResolver;
            _backgroundServiceResolver = backgroundServiceResolver;
        }

        public Task Publish<T>(T eventMessage, bool runInBackground = false) where T : class, IEventMessage
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

            return Task.Run(async () => await PublishEvent(eventMessage, true));
        }

        private async Task PublishEvent<T>(T eventMessage, bool runInBackground = false) where T : class, IEventMessage
        {
            var serviceProviderToUse = !runInBackground
                                           ? _serviceResolver
                                           : _backgroundServiceResolver;

            using var serviceScope = serviceProviderToUse.CreateScope();

            var eventHandlers = serviceScope.ServiceProvider
                                            .GetServices<IEventHandler<T>>()
                                            .OrderByDescending(_ => _.Priority)
                                            .ToArray();

            if (!eventHandlers.Any())
                return;

            if (eventMessage is INonStoppedEventMessage)
            {
                await Task.WhenAll(eventHandlers.Select(_ => PublishEvent(_, eventMessage)));

                return;
            }

            foreach (var eventHandler in eventHandlers)
            {
                var shouldContinue = await PublishEvent(eventHandler, eventMessage);
                if (!shouldContinue)
                    break;
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
                _logger.LogWarning("Event handler not implemented.", exception);
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Error while executing event {typeof(T)}", exception);
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
}