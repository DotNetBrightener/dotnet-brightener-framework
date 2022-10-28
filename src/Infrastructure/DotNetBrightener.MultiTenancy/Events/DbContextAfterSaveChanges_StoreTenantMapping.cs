using DotNetBrightener.DataAccess.EF.Events;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.MultiTenancy.Entities;
using DotNetBrightener.MultiTenancy.Services;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetBrightener.MultiTenancy.BackgroundTaskService;

// ReSharper disable InconsistentNaming
namespace DotNetBrightener.MultiTenancy.Events;

internal class DbContextAfterSaveChanges_StoreTenantMapping
    : IEventHandler<DbContextAfterSaveChanges>
{
    private readonly IBackgroundTaskScheduler _taskScheduler;
    private readonly ITenantAccessor          _tenantAccessor;
    private readonly Lazy<DbContext>          _dbContext;

    public DbContextAfterSaveChanges_StoreTenantMapping(IBackgroundTaskScheduler taskScheduler,
                                                        ITenantAccessor          tenantAccessor,
                                                        Lazy<DbContext>          dbContext)
    {
        _taskScheduler  = taskScheduler;
        _tenantAccessor = tenantAccessor;
        _dbContext      = dbContext;
    }

    public int Priority => 0;

    public Task<bool> HandleEvent(DbContextAfterSaveChanges eventMessage)
    {
        // ignore if no record being inserted
        if (eventMessage.InsertedEntityEntries.Length == 0)
            return Task.FromResult(true);

        // ignore if no tenant mapping is available
        if (TenantSupportedRepository.HasTenantMapping == false)
            return Task.FromResult(true);

        var listOfTenantsToLimits = _tenantAccessor.CurrentTenantIds.Any()
                                        ? _tenantAccessor.CurrentTenantIds
                                        : _tenantAccessor.LimitedTenantIdsToRecordsPersistence;

        // ignore if no tenant limit is available
        if (listOfTenantsToLimits.Length == 0)
            return Task.FromResult(true);

        List<TenantEntityMapping> tenantEntityMappings = new();

        // populate data
        foreach (EntityEntry entityEntry in eventMessage.InsertedEntityEntries)
        {
            var entityType = entityEntry.Entity.GetType();

            if (MultiTenantConfiguration.ShouldIgnoreTenantMapping(entityType) ||
                entityEntry.Entity is not BaseEntity entity)
                continue;

            var mappings = listOfTenantsToLimits.Select(_ => new TenantEntityMapping
            {
                TenantId   = _,
                EntityId   = entity.Id,
                EntityType = MultiTenantConfiguration.GetEntityType(entityType)
            });

            tenantEntityMappings.AddRange(mappings);
        }

        // no records to map to tenant, just exit from here
        if (tenantEntityMappings.Count == 0)
            return Task.FromResult(true);

        // enqueue in a separate task to not block the current thread
        var methodInfo = this.GetMethodWithName(nameof(AssignTenantMapping));

        _taskScheduler.EnqueueTask(methodInfo,
                                   tenantEntityMappings);

        return Task.FromResult(true);
    }

    private async Task AssignTenantMapping(IEnumerable<TenantEntityMapping> tenantMappingEntries)
    {
        await _dbContext.Value
                        .Set<TenantEntityMapping>()
                        .AddRangeAsync(tenantMappingEntries);

        await _dbContext.Value.SaveChangesAsync();
    }
}