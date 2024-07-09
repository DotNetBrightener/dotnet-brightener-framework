using DotNetBrightener.DataAccess.Models;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming

namespace DotNetBrightener.DataAccess.EF.Events;

public static class DbOnBeforeSaveChanges_SetAuditInformation
{
    private const string createdDatePropName   = nameof(BaseEntityWithAuditInfo.CreatedDate);
    private const string createdByPropName     = nameof(BaseEntityWithAuditInfo.CreatedBy);
    private const string lastUpdatedByPropName = nameof(BaseEntityWithAuditInfo.ModifiedBy);
    private const string lastUpdatedPropName   = nameof(BaseEntityWithAuditInfo.ModifiedDate);

    public static void HandleEvent(DbContextBeforeSaveChanges eventMessage,
                                   IDateTimeProvider?         dateTimeProvider,
                                   ILogger                    logger)
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
                    entry.Property(createdDatePropName).CurrentValue =
                        dateTimeProvider?.UtcNowWithOffset ?? DateTimeOffset.Now;
                }
            }
            catch (Exception error)
            {
                logger.LogWarning(error, "Error while trying to set Audit information");
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
                logger.LogWarning(error, "Error while trying to set Audit information");
            }
        }
    }
}