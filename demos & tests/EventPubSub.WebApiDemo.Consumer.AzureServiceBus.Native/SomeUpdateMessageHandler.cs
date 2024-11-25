using DotNetBrightener.Plugins.EventPubSub;
using EventPubSub.WebApiDemo.Contracts;

namespace EventPubSub.WebApiDemo.Consumer.AzureServiceBus.Native;

public class SomeUpdateMessageHandler(ILogger<SomeUpdateMessageHandler> logger)
    : DistributedEventHandler<SomeUpdateMessage>
{
    public override Task<bool> HandleEvent(SomeUpdateMessage eventMessage)
    {
        logger.LogInformation("SomeUpdateMessageHandler: {Name}, received from {originalApp}",
                              eventMessage.Name,
                              eventMessage.OriginApp);

        return Task.FromResult<bool>(true);
    }
}