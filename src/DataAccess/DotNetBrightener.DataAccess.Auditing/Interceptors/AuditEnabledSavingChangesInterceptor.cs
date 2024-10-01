#nullable enable
using DotNetBrightener.DataAccess.Auditing.Entities;
using DotNetBrightener.DataAccess.Auditing.EventMessages;
using DotNetBrightener.DataAccess.Auditing.Internal;
using DotNetBrightener.DataAccess.Models.Auditing;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.Auditing.Interceptors;

internal class AuditEnabledSavingChangesInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    private readonly IAuditEntriesContainer          _auditEntriesContainer           = serviceProvider.GetRequiredService<IAuditEntriesContainer>();
    private readonly IEventPublisher?                _eventPublisher                  = serviceProvider.TryGet<IEventPublisher>();
    private readonly IHttpContextAccessor?           _httpContextAccessor             = serviceProvider.TryGet<IHttpContextAccessor>();
    private readonly ICurrentLoggedInUserResolver?   _currentLoggedInUserResolver     = serviceProvider.TryGet<ICurrentLoggedInUserResolver>();
    private readonly IgnoreAuditingEntitiesContainer _ignoreAuditingEntitiesContainer = serviceProvider.GetRequiredService<IgnoreAuditingEntitiesContainer>();
    
    private readonly Guid _scopeId = Ulid.NewUlid().ToGuid();

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

        var startAction   = DateTimeOffset.UtcNow;
        var url           = _httpContextAccessor?.HttpContext?.Request.GetDisplayUrl();
        var requestMethod = _httpContextAccessor?.HttpContext?.Request.Method;

        foreach (var entityEntry in entityEntries)
        {
            var recordType = entityEntry.Entity.GetType();

            if (recordType.Namespace?.StartsWith("Castle.Proxies") == true &&
                recordType.BaseType is not null)
            {
                recordType = recordType.BaseType;
            }

            if (_ignoreAuditingEntitiesContainer.Contains(recordType))
            {
                continue;
            }

            List<string> primaryKeys = entityEntry.Metadata
                                                  .FindPrimaryKey()
                                                 ?.Properties
                                                  .Select(x => x.Name)
                                                  .ToList() ?? new();

            var isAdded = entityEntry.State == EntityState.Added;
            var isDeleted = entityEntry.State == EntityState.Deleted;

            var auditProperties = entityEntry.Properties
                                             .Where(p => !primaryKeys.Contains(p.Metadata.Name))
                                             .Select(p => new AuditProperty
                                              {
                                                  PropertyName = p.Metadata.Name,
                                                  OldValue     = isAdded ? null : p.OriginalValue,
                                                  NewValue     = isDeleted ? null : p.CurrentValue
                                              })
                                             .OrderBy(p => p.PropertyName)
                                             .ToList();

            var auditEntity = new AuditEntity
            {
                Id                    = Ulid.NewUlid().ToGuid(),
                ScopeId               = _scopeId,
                StartTime             = startAction,
                Action                = entityEntry.State.ToString(),
                EntityType            = recordType.Name,
                EntityTypeFullName    = recordType.FullName,
                Url                   = $"{requestMethod} {url}".Trim(),
                UserName              = _currentLoggedInUserResolver?.CurrentUserName ?? "Not Detected",
                AssociatedEntityEntry = entityEntry,
                AuditProperties       = auditProperties.ToImmutableList()
            };

            PrepareDebugView(auditEntity);

            _auditEntriesContainer.AuditEntries.Add(auditEntity);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData,
                                                           int                           result,
                                                           CancellationToken             cancellationToken = default)
    {
        if (eventData.Context is null ||
            _auditEntriesContainer.AuditEntries.Count == 0)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        var endTime = DateTimeOffset.UtcNow;

        foreach (var auditEntity in _auditEntriesContainer.AuditEntries)
        {
            var dictionary = new Dictionary<string, object?>();

            var primaryKeys = auditEntity.AssociatedEntityEntry
                                         .Metadata
                                         .FindPrimaryKey()
                                        ?.Properties
                                         .ToList();

            if (primaryKeys is not null &&
                primaryKeys.Count > 0)
            {
                foreach (var primaryKey in primaryKeys)
                {
                    dictionary.Add(primaryKey.Name,
                                   auditEntity.AssociatedEntityEntry.Property(primaryKey.Name).CurrentValue);
                }
            }

            auditEntity.EntityIdentifier = JsonConvert.SerializeObject(dictionary, Formatting.None);
            auditEntity.EndTime          = endTime;
            auditEntity.Duration         = endTime.Subtract(auditEntity.StartTime!.Value);
            auditEntity.IsSuccess        = true;
        }

        if (_eventPublisher is not null)
            await _eventPublisher.Publish(new AuditTrailMessage
                                          {
                                              AuditEntities = [.. _auditEntriesContainer.AuditEntries]
                                          },
                                          runInBackground: true);

        _auditEntriesContainer.AuditEntries.Clear();

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
                                                      CancellationToken       cancellationToken = default)
    {

        if (eventData.Context is null ||
            _auditEntriesContainer.AuditEntries.Count == 0)
        {
            return;
        }

        var endTime = DateTimeOffset.UtcNow;


        foreach (var auditEntity in _auditEntriesContainer.AuditEntries)
        {
            var dictionary = new Dictionary<string, object?>();

            var primaryKeys = auditEntity.AssociatedEntityEntry
                                         .Metadata
                                         .FindPrimaryKey()
                                        ?.Properties
                                         .ToList();

            if (primaryKeys is not null &&
                primaryKeys.Count > 0)
            {
                foreach (var primaryKey in primaryKeys)
                {
                    dictionary.Add(primaryKey.Name,
                                   auditEntity.AssociatedEntityEntry.Property(primaryKey.Name).CurrentValue);
                }
            }

            auditEntity.EntityIdentifier = JsonConvert.SerializeObject(dictionary, Formatting.None);
            auditEntity.EndTime          = endTime;
            auditEntity.Duration         = endTime.Subtract(auditEntity.StartTime!.Value);
            auditEntity.IsSuccess        = false;
            auditEntity.Exception        = eventData.Exception.GetFullExceptionMessage();
        }

        if (_eventPublisher is not null)
            await _eventPublisher.Publish(new AuditTrailMessage
                                          {
                                              AuditEntities = [.._auditEntriesContainer.AuditEntries]
                                          },
                                          runInBackground: true);

        _auditEntriesContainer.AuditEntries.Clear();
    }

    private void PrepareDebugView(AuditEntity auditEntity)
    {
        // Create a stack trace with file info enabled
        var stackTrace = new StackTrace(0, true);

        // Filter out frames that belong to EF Core and System namespaces
        var frames = stackTrace.GetFrames()
                               .Where(frame =>
                                {
                                    var method        = frame.GetMethod();
                                    var declaringType = method?.DeclaringType;
                                    var namespaceName = declaringType?.Namespace;

                                    return !declaringType!.FullName
                                                          .StartsWith(typeof(AuditEnabledSavingChangesInterceptor)
                                                                         .FullName) &&
                                           namespaceName != null &&
                                           !namespaceName.StartsWith("Microsoft.EntityFrameworkCore") &&
                                           !namespaceName.StartsWith("System");
                                })
                               .Take(10) // Take the first 10 frames outside of EF Core and System
                               .ToList();

        foreach (var frame in frames)
        {
            var method        = frame.GetMethod();
            var declaringType = method?.DeclaringType;
            var className     = declaringType?.FullName;
            var methodName    = method?.Name;
            var lineNumber    = frame.GetFileLineNumber();

            auditEntity.DebugView +=
                $"{className}->{methodName}@{lineNumber} {Environment.NewLine}";
        }
    }
}