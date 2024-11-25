using DotNetBrightener.DataAccess.Events;
using DotNetBrightener.Plugins.EventPubSub;
using FluentAssertions;

namespace DotNetBrightener.DataAccess.EF.Tests.RepositoryTests_CRUD_Operations;

public class TestEntityOnUpdatedEventHandler(IMockAwaiter  awaiter) : IEventHandler<EntityUpdated<TestEntity>>
{
    public int Priority => 100;

    public async Task<bool> HandleEvent(EntityUpdated<TestEntity> eventMessage)
    {
        eventMessage.Entity.Should().NotBeNull();

        var expectedData = new TestEntity();

        eventMessage.Entity.CopyTo(expectedData);

        expectedData.Name = eventMessage.Entity!.Name + "_Updated by update event handler";

        awaiter.WaitFinished(expectedData);

        return true;
    }
}