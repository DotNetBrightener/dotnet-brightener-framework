using DotNetBrightener.DataAccess.Auditing.EventMessages;
using DotNetBrightener.DataAccess.Auditing.Storage.DbContexts;
using DotNetBrightener.Plugins.EventPubSub;
using LinqToDB.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.Auditing.Storage.EventHandlers;

internal class SaveAuditTrailService : IEventHandler<AuditTrailMessage>
{
    private readonly ILogger                       _logger;
    private readonly MssqlStorageAuditingDbContext _dbContext;

    public SaveAuditTrailService(MssqlStorageAuditingDbContext dbContext, ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _logger    = loggerFactory.CreateLogger(GetType());
    }

    public int Priority => 10_000;

    public async Task<bool> HandleEvent(AuditTrailMessage eventMessage)
    {
        try
        {
            await _dbContext.BulkCopyAsync(eventMessage.AuditEntities);
            
            _logger.LogInformation("Save {records} audit entries using bulk copy.\r\n" +
                                   "Audit entries: [@{auditEntries}].",
                                   eventMessage.AuditEntities.Count,
                                   eventMessage.AuditEntities);
        }
        catch (Exception ex)
        {
            try
            {
                await _dbContext.AddRangeAsync(eventMessage.AuditEntities);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Save {records} audit entries using AddRange.\r\n" +
                                       "Audit entries: [@{auditEntries}].",
                                       eventMessage.AuditEntities.Count,
                                       eventMessage.AuditEntities);
            }
            catch (Exception ex2)
            {
                _logger.LogError(ex2,
                                 "Error while trying to save audit entries.\r\n" +
                                 "Audit entries: [@{auditEntries}]",
                                 eventMessage.AuditEntities);
                return false;
            }
        }

        return true;
    }
}