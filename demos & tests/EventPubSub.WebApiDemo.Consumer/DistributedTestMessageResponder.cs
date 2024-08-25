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