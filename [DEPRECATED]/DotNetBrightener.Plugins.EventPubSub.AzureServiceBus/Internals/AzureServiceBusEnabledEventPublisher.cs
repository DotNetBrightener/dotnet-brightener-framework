using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Internals;

internal class AzureServiceBusEnabledEventPublisher(
    IServiceScopeFactory serviceScopeFactory,
    ILoggerFactory loggerFactory)
    : DefaultEventPublisher(serviceScopeFactory, loggerFactory)
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public override async Task Publish<T>(T eventMessage,
                                          bool runInBackground = false,
                                          EventMessageWrapper originMessage = null)
    {
        if (eventMessage is DistributedEventMessage message)
        {
            Task.Run(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();

                var serviceBusMessagePublisher =
                    scope.ServiceProvider.GetRequiredService<IServiceBusMessagePublisher>();

                await serviceBusMessagePublisher.Publish(message, originMessage);
            });
        }
        else
        {
            await base.Publish(eventMessage, runInBackground);
        }
    }
}