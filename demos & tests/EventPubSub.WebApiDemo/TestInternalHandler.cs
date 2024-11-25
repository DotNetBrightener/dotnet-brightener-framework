using DotNetBrightener.Plugins.EventPubSub;
using EventPubSub.WebApiDemo.Contracts;

namespace EventPubSub.WebApiDemo;

internal class TestInternalHandler : IEventHandler<TestMessage>
{
    public int Priority => 1000;

    private readonly ILogger _logger;

    public TestInternalHandler(ILogger<TestInternalHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HandleEvent(TestMessage eventMessage)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));

        _logger.LogInformation($"Received message: {eventMessage.Name}");

        eventMessage.Name += " updated by handler";

        return true;
    }
}