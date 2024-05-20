using DotNetBrightener.Plugins.EventPubSub;
using EventPubSub.WebApiDemo.Contracts;

namespace EventPubSub.WebApiDemo;

internal class TestHandler : IEventHandler<TestMessage>
{
    public int Priority => 1000;

    public Task<bool> HandleEvent(TestMessage eventMessage)
    {
        Console.WriteLine($"Received message: {eventMessage.Name}");

        eventMessage.Name += " updated by handler";

        return Task.FromResult<bool>(true);
    }
}