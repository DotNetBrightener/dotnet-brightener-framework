using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;

internal class AzureServiceBusEnabledEventPublisher(
    IServiceScopeFactory        serviceResolver,
    ILoggerFactory              loggerFactory,
    IServiceBusMessagePublisher serviceBusMessagePublisher)
    : DefaultEventPublisher(serviceResolver, loggerFactory)
{
    public override async Task Publish<T>(T eventMessage, bool runInBackground = false)
    {
        if (eventMessage is IDistributedEventMessage message)
        {
            await serviceBusMessagePublisher.SendMessageAsync(message);
        }
        else
        {
            await base.Publish(eventMessage, runInBackground);
        }
    }
}