#nullable enable
using DotNetBrightener.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Uuid7 = DotNetBrightener.DataAccess.Models.Utils.Internal.Uuid7;

namespace DotNetBrightener.DataAccess.EF.Interceptors;

internal class AuditInformationFillerInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    private readonly IDateTimeProvider? _dateTimeProvider = serviceProvider.TryGet<IDateTimeProvider>();
    private readonly ILogger?           _logger = serviceProvider.TryGet<ILogger<AuditInformationFillerInterceptor>>();

    private readonly ICurrentLoggedInUserResolver? _currentLoggedInUserResolver =
        serviceProvider.TryGet<ICurrentLoggedInUserResolver>();

    private readonly Guid _scopeId = Uuid7.Guid();

    private const string CreatedDatePropName   = nameof(BaseEntityWithAuditInfo.CreatedDate);
    private const string CreatedByPropName     = nameof(BaseEntityWithAuditInfo.CreatedBy);
    private const string LastUpdatedByPropName = nameof(BaseEntityWithAuditInfo.ModifiedBy);
    private const string LastUpdatedPropName   = nameof(BaseEntityWithAuditInfo.ModifiedDate);

    private const string CastleProxies = "Castle.Proxies.";

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData      eventData,
                                                                                InterceptionResult<int> result,
                                                                                CancellationToken cancellationToken =
                                                                                    default)
    {
        if (eventData.Context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entityEntries = eventData.Context.ChangeTracker
                                     .Entries()
                                     .ToArray();

        if (entityEntries.Length == 0)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        foreach (var entry in entityEntries)
        {
            if (entry.State is not EntityState.Added
                and not EntityState.Modified
                and not EntityState.Deleted)
            {
                continue;
            }

            var entityType = entry.Entity.GetType();
            var actualEntityType = entityType.FullName?.StartsWith(CastleProxies) == true
                                       ? entityType.BaseType
                                       : entityType;

            var currentUserName = _currentLoggedInUserResolver?.CurrentUserName ??
                                  _currentLoggedInUserResolver?.CurrentUserId;
            var utcNow = _dateTimeProvider?.UtcNowWithOffset ?? DateTimeOffset.UtcNow;

            if (entry.State is EntityState.Added)
            {
                _logger?.LogDebug("Entity {entityType} is created", actualEntityType);

                if (entry.Properties.Any(p => p.Metadata.Name == CreatedByPropName) &&
                    entry.Property(CreatedByPropName).CurrentValue == null)
                {
                    _logger?.LogDebug("Assigning {property} property value => {value}",
                                      CreatedByPropName,
                                      currentUserName);
                    entry.Property(CreatedByPropName).CurrentValue = currentUserName ?? "[Not Detected]";
                }

                if (entry.Properties.Any(p => p.Metadata.Name == CreatedDatePropName) &&
                    entry.Property(CreatedDatePropName).CurrentValue == null)
                {
                    _logger?.LogDebug("Assigning {property} property value => {value}",
                                      CreatedDatePropName,
                                      utcNow);
                    entry.Property(CreatedDatePropName).CurrentValue = utcNow;
                }
            }

            if (entry.State is EntityState.Modified)
            {
                _logger?.LogDebug("Entity {entityType} is updated", actualEntityType);

                if (entry.Properties.Any(p => p.Metadata.Name == LastUpdatedByPropName))
                {
                    _logger?.LogDebug("Assigning {property} property value => {value}",
                                      LastUpdatedByPropName,
                                      currentUserName);
                    entry.Property(LastUpdatedByPropName).CurrentValue = currentUserName ?? "[Not Detected]";
                }

                if (entry.Properties.Any(p => p.Metadata.Name == LastUpdatedPropName))
                {
                    _logger?.LogDebug("Assigning {property} property value => {value}",
                                      LastUpdatedPropName,
                                      utcNow);
                    entry.Property(LastUpdatedPropName).CurrentValue = utcNow;
                }
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}