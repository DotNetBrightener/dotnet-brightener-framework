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

    public Task<bool> HandleEvent(TestMessage eventMessage)
    {
        _logger.LogInformation($"Received message: {eventMessage.Name}");

        eventMessage.Name += " updated by handler";

        return Task.FromResult<bool>(true);
    }
}