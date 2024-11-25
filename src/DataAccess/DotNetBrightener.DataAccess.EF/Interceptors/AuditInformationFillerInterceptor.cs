#nullable enable
using DotNetBrightener.DataAccess.EF.Events;
using DotNetBrightener.DataAccess.Events;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Uuid7 = DotNetBrightener.DataAccess.Models.Utils.Internal.Uuid7;

namespace DotNetBrightener.DataAccess.EF.Interceptors;

internal class AuditInformationFillerInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    private readonly IDateTimeProvider? _dateTimeProvider = serviceProvider.TryGet<IDateTimeProvider>();
    private readonly ILogger?           _logger = serviceProvider.TryGet<ILogger<AuditInformationFillerInterceptor>>();
    private readonly IEventPublisher?   _eventPublisher = serviceProvider.TryGet<IEventPublisher>();

    private readonly IInterceptorsEntriesContainer? _entriesContainer =
        serviceProvider.TryGet<IInterceptorsEntriesContainer>();

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

                _entriesContainer?.InsertedEntityEntries.Add(entry);
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

                _entriesContainer?.ModifiedEntityEntries.Add(entry);
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData,
                                                           int                           result,
                                                           CancellationToken cancellationToken =
                                                               new CancellationToken())
    {
        if (_eventPublisher is not null &&
            _entriesContainer is not null)
        {
            var insertedEntities = _entriesContainer.InsertedEntityEntries;
            var updatedEntities  = _entriesContainer.ModifiedEntityEntries;


            var eventMessages = new List<IEventMessage>();

            ProcessEntitiesEvent(insertedEntities, eventMessages, typeof(EntityCreated<>));

            ProcessEntitiesEvent(updatedEntities, eventMessages, typeof(EntityUpdated<>));

            if (eventMessages.Any())
            {
                await eventMessages.ParallelForEachAsync(eventMsg =>
                {
                    return _eventPublisher.Publish(eventMsg, runInBackground: true);
                });
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void ProcessEntitiesEvent(List<EntityEntry>   entityEntries,
                                      List<IEventMessage> eventMessages,
                                      Type                eventType)
    {
        if (!entityEntries.Any())
        {
            return;
        }

        var entityTypes = entityEntries.Select(entry => entry.Entity.GetType())
                                       .Distinct()
                                       .ToArray();

        foreach (var entityType in entityTypes)
        {
            var actualEntityType = entityType.FullName?.StartsWith(CastleProxies) == true
                                       ? entityType.BaseType
                                       : entityType;

            var eventMessageType = eventType.MakeGenericType(actualEntityType!);

            var entries = entityEntries.Where(entry => entry.Entity.GetType() == entityType)
                                       .ToArray();

            foreach (var record in entries)
            {
                if (Activator.CreateInstance(eventMessageType,
                                             record.Entity,
                                             _currentLoggedInUserResolver?.CurrentUserId,
                                             _currentLoggedInUserResolver?.CurrentUserName) is IEventMessage
                    entityEventMsg)
                {
                    eventMessages.Add(entityEventMsg);
                }
            }
        }
    }
}