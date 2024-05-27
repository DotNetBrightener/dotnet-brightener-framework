using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming

namespace DotNetBrightener.DataAccess.EF.Events;

public class DbOnBeforeSaveChanges_SetAuditInformation(
    ILogger<DbOnBeforeSaveChanges_SetAuditInformation> logger,
    IDateTimeProvider                                  dateTimeProvider)
    : IEventHandler<DbContextBeforeSaveChanges>
{
    public int Priority => 0;

    private readonly ILogger _logger = logger;

    private const string createdDatePropName   = nameof(BaseEntityWithAuditInfo.CreatedDate);
    private const string createdByPropName     = nameof(BaseEntityWithAuditInfo.CreatedBy);
    private const string lastUpdatedByPropName = nameof(BaseEntityWithAuditInfo.ModifiedBy);
    private const string lastUpdatedPropName   = nameof(BaseEntityWithAuditInfo.ModifiedDate);

    public Task<bool> HandleEvent(DbContextBeforeSaveChanges eventMessage)
    {
        foreach (var entry in eventMessage.InsertedEntityEntries)
        {
            try
            {
                if (entry.Properties.Any(p => p.Metadata.Name == createdByPropName) &&
                    entry.Property(createdByPropName).CurrentValue == null)
                {
                    entry.Property(createdByPropName).CurrentValue =
                        eventMessage.CurrentUserName ??
                        eventMessage.CurrentUserId;
                }

                if (entry.Properties.Any(p => p.Metadata.Name == createdDatePropName) &&
                    entry.Property(createdDatePropName).CurrentValue == null)
                {
                    entry.Property(createdDatePropName).CurrentValue = dateTimeProvider.UtcNow;
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
                if (entry.Properties.Any(p => p.Metadata.Name == lastUpdatedByPropName) &&
                    entry.Property(lastUpdatedByPropName).CurrentValue == null)
                {
                    entry.Property(lastUpdatedByPropName).CurrentValue =
                        eventMessage.CurrentUserName ??
                        eventMessage.CurrentUserId;
                }

                if (entry.Properties.Any(p => p.Metadata.Name == lastUpdatedPropName) &&
                    entry.Property(lastUpdatedPropName).CurrentValue == null)
                {
                    entry.Property(lastUpdatedPropName).CurrentValue = dateTimeProvider.UtcNow;
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