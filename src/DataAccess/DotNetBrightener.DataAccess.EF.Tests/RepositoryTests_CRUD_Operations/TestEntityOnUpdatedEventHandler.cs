using DotNetBrightener.DataAccess.Events;
using DotNetBrightener.Plugins.EventPubSub;
using NUnit.Framework;

namespace DotNetBrightener.DataAccess.EF.Tests.RepositoryTests_CRUD_Operations;

public class TestEntityOnUpdatedEventHandler(TestDbContext dbContext) : IEventHandler<EntityUpdated<TestEntity>>
{
    public int Priority => 100;

    public async Task<bool> HandleEvent(EntityUpdated<TestEntity> eventMessage)
    {
        Assert.That(eventMessage.Entity, Is.Not.Null);

        eventMessage.Entity.Name = $"{eventMessage.Entity.Name}_Updated by update event handler";

        dbContext.Set<TestEntity>().Update(eventMessage.Entity);
        await dbContext.SaveChangesAsync();

        return true;
    }
}

public class TestEntityOnUpdatedByExpressionEventHandler(TestDbContext dbContext)
    : IEventHandler<EntityUpdatedByExpression<TestEntity>>
{
    public int Priority => 100;

    public async Task<bool> HandleEvent(EntityUpdatedByExpression<TestEntity> eventMessage)
    {

        return true;
    }
}