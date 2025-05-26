using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Plugins.EventPubSub.Distributed;

public class DistributedEventPublisher(
    IServiceScopeFactory serviceScopeFactory,
    ILoggerFactory       loggerFactory)
    : DefaultEventPublisher(serviceScopeFactory, loggerFactory)
{
    internal readonly IServiceScopeFactory ServiceScopeFactory = serviceScopeFactory;

    public override async Task Publish<T>(T                   eventMessage,
                                          bool                runInBackground = false,
                                          EventMessageWrapper originMessage   = null)
    {
        if (eventMessage is DistributedEventMessage message)
        {
            using var scope = ServiceScopeFactory.CreateScope();

            var serviceBusMessagePublisher =
                scope.ServiceProvider.GetRequiredService<IDistributedMessagePublisher>();

            await serviceBusMessagePublisher.Publish(message, originMessage);
        }
        else
        {
            await base.Publish(eventMessage, runInBackground);
        }
    }
}