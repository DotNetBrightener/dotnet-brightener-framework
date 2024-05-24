using DotNetBrightener.Core.BackgroundTasks;
using DotNetBrightener.DataAccess.EF.Events;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.MultiTenancy.Entities;
using DotNetBrightener.MultiTenancy.Services;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

// ReSharper disable InconsistentNaming
namespace DotNetBrightener.MultiTenancy.Events;

internal class DbContextAfterSaveChanges_StoreTenantMapping(
    ITenantAccessor tenantAccessor,
    Lazy<DbContext> dbContext,
    IScheduler      scheduler)
    : IEventHandler<DbContextAfterSaveChanges>
{
    public int Priority => 0;

    public Task<bool> HandleEvent(DbContextAfterSaveChanges eventMessage)
    {
        // ignore if no record being inserted
        if (eventMessage.InsertedEntityEntries.Length == 0 ||
            // ignore if no tenant mapping is available
            TenantSupportedRepository.HasTenantMapping == false)
            return Task.FromResult(true);

        var listOfTenantsToLimits = tenantAccessor.CurrentTenantIds.Any()
                                        ? tenantAccessor.CurrentTenantIds
                                        : tenantAccessor.LimitedTenantIdsToRecordsPersistence;

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

        scheduler.ScheduleTaskOnce(methodInfo, tenantEntityMappings);

        return Task.FromResult(true);
    }

    private async Task AssignTenantMapping(IEnumerable<TenantEntityMapping> tenantMappingEntries)
    {
        await dbContext.Value
                       .Set<TenantEntityMapping>()
                       .AddRangeAsync(tenantMappingEntries);

        await dbContext.Value.SaveChangesAsync();
    }
}