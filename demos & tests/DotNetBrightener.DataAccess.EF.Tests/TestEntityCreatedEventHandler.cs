using DotNetBrightener.DataAccess.Events;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.EF.Tests;

internal class TestEntityCreatedEventHandler: IEventHandler<EntityCreated<TestEntity>>,
                                              IEventHandler<EntityUpdatedByExpression<TestEntity>>
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

    public async Task<bool> HandleEvent(EntityUpdatedByExpression<TestEntity> eventMessage)
    {
        _logger.LogInformation("Entity filtered by query {query}, updated by expression {updateExpression}. Affected records: {affectedRecords}",
                               eventMessage.FilterExpression,
                               eventMessage.UpdateExpression,
                               eventMessage.AffectedRecords);
        return true;
    }
}