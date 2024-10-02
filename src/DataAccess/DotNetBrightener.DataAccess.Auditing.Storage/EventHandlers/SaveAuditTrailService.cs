using System.Diagnostics.Eventing.Reader;
using DotNetBrightener.DataAccess.Auditing.Entities;
using DotNetBrightener.DataAccess.Auditing.Storage.DbContexts;
using DotNetBrightener.DataAccess.EF.Auditing;
using DotNetBrightener.Plugins.EventPubSub;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
        foreach (var auditEntry in eventMessage.AuditEntities)
        {
            auditEntry.Changes = JsonConvert.SerializeObject(auditEntry.AuditProperties);
        }

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