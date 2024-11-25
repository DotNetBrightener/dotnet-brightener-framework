using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub;

public class DefaultEventPublisher : IEventPublisher, IDisposable
{
    private readonly IServiceScopeFactory      _serviceScopeFactory;
    private readonly InMemoryEventMessageQueue _eventMessageQueue;
    private readonly IServiceScope             _serviceScope;
    private readonly ILogger                   _logger;

    public DefaultEventPublisher(IServiceScopeFactory serviceScopeFactory,
                                 ILoggerFactory       loggerFactory)
    {
        _logger              = loggerFactory.CreateLogger(GetType());
        _serviceScopeFactory = serviceScopeFactory;
        _serviceScope        = serviceScopeFactory.CreateScope();
        _eventMessageQueue   = _serviceScope.ServiceProvider
                                            .GetRequiredService<InMemoryEventMessageQueue>();
    }

    public virtual Task Publish<T>(T                   eventMessage,
                                   bool                runInBackground = false,
                                   EventMessageWrapper originMessage   = null) where T : class, IEventMessage
    {
        if (!runInBackground)
        {
            _logger.LogDebug("Processing message {type} in same process thread", typeof(T).Name);

            using var serviceScope = _serviceScopeFactory.CreateScope();

            var eventProcessor = serviceScope.ServiceProvider
                                             .GetRequiredService<InMemoryEventProcessor<T>>();

            return eventProcessor.ProcessEventMessage(eventMessage);
        }

        _logger.LogDebug("Processing message {type} in background", typeof(T).Name);
        return _eventMessageQueue.Enqueue(eventMessage);
    }


    public void Dispose()
    {
        _serviceScope?.Dispose();
    }
}