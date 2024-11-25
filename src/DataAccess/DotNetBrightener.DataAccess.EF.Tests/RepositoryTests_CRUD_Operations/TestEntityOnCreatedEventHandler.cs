using DotNetBrightener.DataAccess.Events;
using DotNetBrightener.Plugins.EventPubSub;
using FluentAssertions;

namespace DotNetBrightener.DataAccess.EF.Tests.RepositoryTests_CRUD_Operations;

public class TestEntityOnCreatedEventHandler(IMockAwaiter awaiter) : IEventHandler<EntityCreated<TestEntity>>
{
    public int Priority => 100;

    public async Task<bool> HandleEvent(EntityCreated<TestEntity> eventMessage)
    {
        eventMessage.Entity.Should().NotBeNull();

        var expectedData = new TestEntity();
        
        eventMessage.Entity.CopyTo(expectedData);

        expectedData.Name = eventMessage.Entity!.Name + "_Created by create event handler";

        awaiter.WaitFinished(expectedData);

        return true;
    }
}