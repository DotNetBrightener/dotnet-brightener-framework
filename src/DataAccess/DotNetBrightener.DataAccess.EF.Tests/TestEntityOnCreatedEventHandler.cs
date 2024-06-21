using DotNetBrightener.DataAccess.Events;
using DotNetBrightener.Plugins.EventPubSub;
using NUnit.Framework;

namespace DotNetBrightener.DataAccess.EF.Tests;

public class TestEntityOnCreatedEventHandler(TestDbContext dbContext) : IEventHandler<EntityCreated<TestEntity>>
{
    public           int           Priority => 100;

    public async Task<bool> HandleEvent(EntityCreated<TestEntity> eventMessage)
    {
        Assert.That(eventMessage.Entity, Is.Not.Null);

        eventMessage.Entity.Name = $"{eventMessage.Entity.Name}_Updated by event handler";

        dbContext.Set<TestEntity>().Update(eventMessage.Entity);
        await dbContext.SaveChangesAsync();

        return true;
    }
}