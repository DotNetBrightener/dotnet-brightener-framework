using DotNetBrightener.DataAccess.Auditing.EventMessages;
using DotNetBrightener.DataAccess.Auditing.Storage.DbContexts;
using DotNetBrightener.Plugins.EventPubSub;

namespace DotNetBrightener.DataAccess.Auditing.Storage.EventHandlers;

internal class SaveAuditTrailService(MssqlStorageAuditingDbContext dbContext) : IEventHandler<AuditTrailMessage>
{
    public int Priority => 10_000;

    public async Task<bool> HandleEvent(AuditTrailMessage eventMessage)
    {
        await dbContext.AddRangeAsync(eventMessage.AuditEntities);
        await dbContext.SaveChangesAsync();

        return true;
    }
}