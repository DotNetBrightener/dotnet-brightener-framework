using DotNetBrightener.DataAccess.EF.Auditing;
using DotNetBrightener.Plugins.EventPubSub;

namespace DotNetBrightener.DataAccess.EF.Tests.RepositoryTests_CRUD_Operations;

public class MockAuditTrailMessageHandler : IEventHandler<AuditTrailMessage>
{
    private readonly IMockAwaiter _awaiter;

    public MockAuditTrailMessageHandler(IMockAwaiter  awaiter)
    {
        _awaiter = awaiter;
    }

    public Task<bool> HandleEvent(AuditTrailMessage eventMessage)
    {
        _awaiter.WaitFinished(eventMessage.AuditEntities);

        return Task.FromResult(true);
    }

    public int Priority => 100;
}