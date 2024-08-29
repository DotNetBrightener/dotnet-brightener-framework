using DotNetBrightener.Plugins.EventPubSub;
using EventPubSub.WebApiDemo.Contracts;

namespace EventPubSub.WebApiDemo.Consumer;

public class DistributedTestMessageResponder(ILogger<DistributedTestMessageResponder> logger)
    : RequestResponder<DistributedTestMessage>
{
    public override async Task<bool> HandleEvent(DistributedTestMessage eventMessage)
    {
        await SendResponse(new DistributedTestMessageResponse
        {
            Name = "Hello there, " + eventMessage.Name + $"!! Now is {DateTimeOffset.Now}"
        });

        return true;
    }
}

public class SomeUpdateMessageHandler: DistributedEventHandler<SomeUpdateMessage>
{
    private readonly ILogger<SomeUpdateMessageHandler> _logger;

    public SomeUpdateMessageHandler(ILogger<SomeUpdateMessageHandler> logger)
    {
        _logger = logger;
    }

    public override Task<bool> HandleEvent(SomeUpdateMessage eventMessage)
    {
        _logger.LogInformation("SomeUpdateMessageHandler: {Name}, received from {originalApp}",
                               eventMessage.Name,
                               eventMessage.OriginApp);

        return Task.FromResult<bool>(true);
    }
}