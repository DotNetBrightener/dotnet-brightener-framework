using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub.Distributed.Services;

internal class DistributedEventPublisher
    : DefaultEventPublisher
{
    internal readonly IServiceScopeFactory                ServiceScopeFactory;
    internal readonly IDistributedEventPubSubConfigurator Configurator;
    private readonly  IPublishEndpoint                    _publishEndpoint;
    private readonly  ILogger                             _logger;

    public DistributedEventPublisher(IServiceScopeFactory                serviceScopeFactory,
                                     ILoggerFactory                      loggerFactory,
                                     IDistributedEventPubSubConfigurator configurator,
                                     IPublishEndpoint                    publishEndpoint)
        : base(serviceScopeFactory, loggerFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
        Configurator        = configurator;
        _publishEndpoint    = publishEndpoint;
        _logger             = loggerFactory.CreateLogger(GetType());
    }

    public override async Task Publish<T>(T                   eventMessage,
                                          bool                runInBackground = false,
                                          EventMessageWrapper originMessage   = null)
    {
        if (eventMessage is DistributedEventMessage distributedEventMessage)
        {
            distributedEventMessage.MachineName = Environment.MachineName;
            distributedEventMessage.CurrentApp  = Configurator.AppName;

            if (string.IsNullOrWhiteSpace(distributedEventMessage.OriginApp))
            {
                distributedEventMessage.OriginApp = Configurator.AppName;
            }

            try
            {
                await _publishEndpoint.Publish(eventMessage);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error while publishing event via distributed system");

                throw;
            }

            return;
        }

        await base.Publish(eventMessage, runInBackground);
    }
}