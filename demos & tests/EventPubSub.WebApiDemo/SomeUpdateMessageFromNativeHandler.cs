using DotNetBrightener.Plugins.EventPubSub;
using EventPubSub.WebApiDemo.Contracts;

namespace EventPubSub.WebApiDemo;

public class SomeUpdateMessageFromNativeHandler(ILogger<SomeUpdateMessageFromNativeHandler> logger)
    : DistributedEventHandler<SomeUpdateMessageFromNative>
{
    public override Task<bool> HandleEvent(SomeUpdateMessageFromNative eventMessage)
    {
        logger.LogInformation("SomeUpdateMessageHandler: {Name}, received from {originalApp}",
                              eventMessage.Name,
                              eventMessage.OriginApp);

        return Task.FromResult<bool>(true);
    }
}