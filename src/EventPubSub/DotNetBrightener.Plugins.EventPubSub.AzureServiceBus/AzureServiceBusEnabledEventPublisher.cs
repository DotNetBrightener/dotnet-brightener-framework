using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;

internal class AzureServiceBusEnabledEventPublisher(
    IServiceScopeFactory        serviceScopeFactory,
    ILoggerFactory              loggerFactory)
    : DefaultEventPublisher(serviceScopeFactory, loggerFactory)
{
    public override async Task Publish<T>(T eventMessage, bool runInBackground = false)
    {
        if (eventMessage is IDistributedEventMessage message)
        {
            Task.Run(async () =>
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var serviceBusMessagePublisher =
                        scope.ServiceProvider.GetRequiredService<IServiceBusMessagePublisher>();

                    await serviceBusMessagePublisher.SendMessageAsync(message);
                }
            });
        }
        else
        {
            await base.Publish(eventMessage, runInBackground);
        }
    }
}