#nullable enable
using DotNetBrightener.DataAccess.Auditing.Entities;
using DotNetBrightener.DataAccess.Auditing.EventMessages;
using DotNetBrightener.DataAccess.Models.Auditing;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DotNetBrightener.DataAccess.Auditing.Interceptors;

public class AuditEnabledSavingChangesInterceptor : SaveChangesInterceptor
{
    internal Guid ScopeId { get; } = Ulid.NewUlid().ToGuid();

    private readonly IAuditEntriesContainer        _auditEntriesContainer;
    private readonly IEventPublisher?              _eventPublisher;
    private readonly IHttpContextAccessor?         _httpContextAccessor;
    private readonly ICurrentLoggedInUserResolver? _currentLoggedInUserResolver;

    public AuditEnabledSavingChangesInterceptor(IServiceProvider serviceProvider)
    {
        _auditEntriesContainer       = serviceProvider.GetRequiredService<IAuditEntriesContainer>();
        _eventPublisher              = serviceProvider.TryGet<IEventPublisher>();
        _httpContextAccessor         = serviceProvider.TryGet<IHttpContextAccessor>();
        _currentLoggedInUserResolver = serviceProvider.TryGet<ICurrentLoggedInUserResolver>();
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData      eventData,
                                                                                InterceptionResult<int> result,
                                                                                CancellationToken cancellationToken =
                                                                                    default)
    {
        if (eventData.Context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var startAction   = DateTimeOffset.UtcNow;
        var url           = _httpContextAccessor?.HttpContext?.Request.GetDisplayUrl();
        var requestMethod = _httpContextAccessor?.HttpContext?.Request.Method;

        var auditEntries = eventData.Context.ChangeTracker
                                    .Entries()
                                    .Where(x => x.State is EntityState.Modified 
                                                    or EntityState.Added
                                                    or EntityState.Deleted)
                                    .Select(x =>
                                     {
                                         object? primaryKeyValue = x.State == EntityState.Added
                                                                       ? "[Generated]"
                                                                       : x.Metadata.FindPrimaryKey()
                                                                         ?.Properties
                                                                          .Select(p => x.Property(p.Name).CurrentValue)
                                                                          .FirstOrDefault();

                                         var changeMetadata = x.Properties
                                                               .Where(p => x.State == EntityState.Added ||
                                                                           p.CurrentValue?.Equals(p.OriginalValue) !=
                                                                           true)
                                                               .Select(p => new AuditProperty
                                                                {
                                                                    PropertyName = p.Metadata.Name,
                                                                    OldValue     = p.OriginalValue,
                                                                    NewValue     = p.CurrentValue
                                                                })
                                                               .OrderBy(p => p.PropertyName)
                                                               .ToList();

                                         var recordType = x.Entity.GetType();

                                         if (recordType.Namespace != null && recordType.Namespace.StartsWith("Castle.Proxies"))
                                         {
                                             recordType = recordType.BaseType;
                                         }

                                         return new AuditEntity
                                         {
                                             ScopeId            = ScopeId,
                                             StartTime          = startAction,
                                             Action             = x.State.ToString(),
                                             EntityType         = recordType.Name,
                                             EntityTypeFullName = recordType.FullName,
                                             EntityIdentifier   = primaryKeyValue?.ToString(),
                                             Changes            = JsonConvert.SerializeObject(changeMetadata),
                                             Url                = $"{requestMethod} {url}",
                                             UserName           = _currentLoggedInUserResolver?.CurrentUserName ?? "Not Detected"
                                         };
                                     })
                                    .ToList();

        if (auditEntries.Count > 0)
        {
            _auditEntriesContainer.AuditEntries.AddRange(auditEntries);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData,
                                                           int                           result,
                                                           CancellationToken             cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        var endTime = DateTimeOffset.UtcNow;

        foreach (var auditEntity in _auditEntriesContainer.AuditEntries)
        {
            auditEntity.EndTime   = endTime;
            auditEntity.Duration  = endTime.Subtract(auditEntity.StartTime!.Value);
            auditEntity.IsSuccess = true;
        }

        if (_eventPublisher is not null)
            await _eventPublisher.Publish(new AuditTrailMessage
                                          {
                                              AuditEntities = _auditEntriesContainer.AuditEntries
                                          },
                                          runInBackground: true);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
                                                      CancellationToken       cancellationToken = default)
    {

        if (eventData.Context is null)
        {
            return;
        }

        var endTime = DateTimeOffset.UtcNow;

        foreach (var auditEntity in _auditEntriesContainer.AuditEntries)
        {
            auditEntity.EndTime   = endTime;
            auditEntity.Duration  = endTime.Subtract(auditEntity.StartTime!.Value);
            auditEntity.IsSuccess = false;
            auditEntity.Exception = eventData.Exception.GetFullExceptionMessage();
        }

        if (_eventPublisher is not null)
            await _eventPublisher.Publish(new AuditTrailMessage
                                          {
                                              AuditEntities = _auditEntriesContainer.AuditEntries
                                          },
                                          runInBackground: true);
    }
}