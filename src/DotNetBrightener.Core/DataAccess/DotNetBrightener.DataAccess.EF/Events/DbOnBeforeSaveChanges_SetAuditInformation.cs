using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming

namespace DotNetBrightener.DataAccess.EF.Events;

public class DbOnBeforeSaveChanges_SetAuditInformation : IEventHandler<DbContextBeforeSaveChanges>
{
    public int Priority => 0;

    private readonly ILogger _logger;

    public DbOnBeforeSaveChanges_SetAuditInformation(ILogger<DbOnBeforeSaveChanges_SetAuditInformation> logger)
    {
        _logger = logger;
    }

    public Task<bool> HandleEvent(DbContextBeforeSaveChanges eventMessage)
    {
        foreach (var entry in eventMessage.InsertedEntityEntries)
        {
            try
            {
                var createdByPropName = nameof(BaseEntityWithAuditInfo.CreatedBy);

                if (entry.Properties.Any(_ => _.Metadata.Name == createdByPropName) &&
                    entry.Property(createdByPropName) != null)
                {
                    entry.Property(createdByPropName).CurrentValue =
                        eventMessage.CurrentUserName ??
                        eventMessage.CurrentUserId;
                }

                var createdDatePropName = nameof(BaseEntityWithAuditInfo.CreatedDate);
                if (entry.Properties.Any(_ => _.Metadata.Name == createdDatePropName) &&
                    entry.Property(createdDatePropName) != null)
                {
                    entry.Property(createdDatePropName).CurrentValue = DateTimeOffset.Now;
                }
            }
            catch (Exception error)
            {
                _logger.LogWarning(error, "Error while trying to set Audit information");
            }
        }

        var modifiedEntries = eventMessage.UpdatedEntityEntries;

        foreach (var entry in modifiedEntries)
        {
            try
            {
                var lastUpdatedByPropName = nameof(BaseEntityWithAuditInfo.ModifiedBy);
                if (entry.Properties.Any(_ => _.Metadata.Name == lastUpdatedByPropName) &&
                    entry.Property(lastUpdatedByPropName) != null)
                {
                    entry.Property(lastUpdatedByPropName).CurrentValue =
                        eventMessage.CurrentUserName ??
                        eventMessage.CurrentUserId;
                }

                var lastUpdatedPropName = nameof(BaseEntityWithAuditInfo.ModifiedDate);
                if (entry.Properties.Any(_ => _.Metadata.Name == lastUpdatedPropName) &&
                    entry.Property(lastUpdatedPropName) != null)
                {
                    entry.Property(lastUpdatedPropName).CurrentValue = DateTimeOffset.Now;
                }
            }
            catch (Exception error)
            {
                _logger.LogWarning(error, "Error while trying to set Audit information");
            }
        }

        return Task.FromResult(true);
    }
}