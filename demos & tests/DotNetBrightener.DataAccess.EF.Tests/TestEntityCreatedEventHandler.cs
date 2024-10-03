using DotNetBrightener.DataAccess.Events;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.EF.Tests;

internal class TestEntityCreatedEventHandler: IEventHandler<EntityCreated<TestEntity>>
{
    public           int     Priority => 1000;
    private readonly ILogger _logger;

    public TestEntityCreatedEventHandler(ILogger<TestEntityCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HandleEvent(EntityCreated<TestEntity> eventMessage)
    {
        _logger.LogInformation("Entity created {@entity}", eventMessage.Entity);
        return true;
    }
}