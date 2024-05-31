using DotNetBrightener.Plugins.EventPubSub;
using EventPubSub.WebApiDemo.Contracts;

namespace EventPubSub.WebApiDemo.Consumer;

public class TestHandler : IEventHandler<TestMessage>
{
    public int Priority => 1000;

    private readonly ILogger _logger;

    public TestHandler(ILogger<TestHandler> logger)
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

public class TestEventHandlerDistributed(ILogger<TestEventHandlerDistributed> logger)
    : DistributedEventEventHandler<DistributedTestMessage>
{
    private readonly ILogger _logger = logger;
    
    public override Task<bool> HandleEvent(DistributedTestMessage eventMessage)
    {
        _logger.LogInformation("Received message: {Name}. Origin Payload: {@originPayload}",
                               eventMessage.Name,
                               OriginPayload);

        return Task.FromResult<bool>(true);
    }
}