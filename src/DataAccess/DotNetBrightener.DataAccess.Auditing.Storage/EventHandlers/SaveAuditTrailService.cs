using System.Collections.Immutable;
using DotNetBrightener.DataAccess.Auditing.Storage.DbContexts;
using DotNetBrightener.DataAccess.EF.Auditing;
using DotNetBrightener.Plugins.EventPubSub;
using LinqToDB.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        var entriesToSave = eventMessage.AuditEntities
                                        .DistinctBy(x => x.Id)
                                        .ToImmutableList();

        foreach (var auditEntry in entriesToSave)
        {
            auditEntry.Changes = JsonConvert.SerializeObject(auditEntry.AuditProperties);
        }

        try
        {
            await _dbContext.BulkCopyAsync(entriesToSave);
            
            _logger.LogInformation("Save {records} audit entries using bulk copy.\r\n" +
                                   "Audit entries: [@{auditEntries}].",
                                   entriesToSave.Count,
                                   entriesToSave);
        }
        catch (Exception ex)
        {
            try
            {
                await _dbContext.AddRangeAsync(entriesToSave);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Save {records} audit entries using AddRange.\r\n" +
                                       "Audit entries: [@{auditEntries}].",
                                       entriesToSave.Count,
                                       entriesToSave);
            }
            catch (Exception ex2)
            {
                _logger.LogError(ex2,
                                 "Error while trying to save audit entries.\r\n" +
                                 "Audit entries: [@{auditEntries}]",
                                 entriesToSave);
                return false;
            }
        }

        return true;
    }
}