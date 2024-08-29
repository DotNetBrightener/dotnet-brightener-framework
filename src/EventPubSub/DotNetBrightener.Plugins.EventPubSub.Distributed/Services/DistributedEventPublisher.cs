using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub.Distributed.Services;

internal class DistributedEventPublisher : DefaultEventPublisher
{
    internal readonly IServiceScopeFactory                ServiceScopeFactory;
    internal readonly IDistributedEventPubSubConfigurator Configurator;
    private readonly  IPublishEndpoint                    _publishEndpoint;

    public DistributedEventPublisher(IServiceScopeFactory                serviceScopeFactory,
                                     IDistributedEventPubSubConfigurator configurator,
                                     IPublishEndpoint                    publishEndpoint,
                                     ILoggerFactory                      loggerFactory)
        : base(serviceScopeFactory, loggerFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
        Configurator        = configurator;
        _publishEndpoint    = publishEndpoint;
    }

    public override async Task Publish<T>(T                   eventMessage,
                                          bool                runInBackground = false,
                                          EventMessageWrapper originMessage   = null)
    {
        if (eventMessage is DistributedEventMessage distributedEventMessage)
        {
            try
            {
                distributedEventMessage.MachineName = Environment.MachineName;
                distributedEventMessage.CurrentApp  = Configurator.AppName;

                if (string.IsNullOrWhiteSpace(distributedEventMessage.OriginApp))
                {
                    distributedEventMessage.OriginApp = Configurator.AppName;
                }

                await _publishEndpoint.Publish(eventMessage);
            }
            catch (Exception)
            {

                throw;
            }
        }
        else
        {
            await base.Publish(eventMessage, runInBackground);
        }
    }
}