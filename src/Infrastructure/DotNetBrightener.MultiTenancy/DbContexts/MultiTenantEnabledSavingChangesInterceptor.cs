#nullable enable
using DotNetBrightener.DataAccess.EF.Migrations;
using DotNetBrightener.MultiTenancy.Entities;
using DotNetBrightener.MultiTenancy.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.MultiTenancy.DbContexts;

internal class MultiTenantEnabledDbContextConfigurator(
    MultiTenantEnabledSavingChangesInterceptor multiTenantEnabledSavingChangesInterceptor) : IDbContextConfigurator
{
    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(multiTenantEnabledSavingChangesInterceptor);
    }
}

internal class MultiTenantEnabledSavingChangesInterceptor(IServiceProvider serviceProvider) : SaveChangesInterceptor
{
    private readonly ITenantAccessor? _tenantAccessor = serviceProvider.TryGet<ITenantAccessor>();

    private readonly IInterceptorsEntriesContainer? _entriesContainer =
        serviceProvider.TryGet<IInterceptorsEntriesContainer>();

    private readonly Guid _scopeId = Guid.CreateVersion7();

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
            if (entry.State is EntityState.Added)
            {
                _entriesContainer?.InsertedEntityEntries.Add(entry);
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData,
                                                           int                           result,
                                                           CancellationToken cancellationToken =
                                                               new CancellationToken())
    {
        if (_entriesContainer is not null &&
            _tenantAccessor is not null)
        {
            var listOfTenantsToLimits = _tenantAccessor.LimitedTenantIdsToRecordsPersistence?.Any() == true
                                            ? _tenantAccessor.LimitedTenantIdsToRecordsPersistence
                                            : [_tenantAccessor.CurrentTenantId ?? Guid.Empty];

            listOfTenantsToLimits = listOfTenantsToLimits.Where(x => x != Guid.Empty)
                                                         .ToArray();

            if (listOfTenantsToLimits.Length == 0)
                return await base.SavedChangesAsync(eventData, result, cancellationToken);

            var insertedEntities = _entriesContainer.InsertedEntityEntries;

            List<TenantEntityMapping> tenantMappings = [];

            foreach (var insertedEntity in insertedEntities)
            {
                var entityType = insertedEntity.Entity.GetType();

                if (MultiTenantConfiguration.ShouldIgnoreTenantMapping(entityType))
                    continue;

                var primaryKey = insertedEntity.Metadata
                                               .FindPrimaryKey()
                                              ?.Properties
                                               .FirstOrDefault(x => x.Name == "Id");


                if (primaryKey is null)
                    continue;

                var primaryKeyValue = insertedEntity.Property(primaryKey.Name).CurrentValue;

                if (primaryKeyValue is null)
                    continue;

                var mappings = listOfTenantsToLimits.Select(tenantId => new TenantEntityMapping
                {
                    TenantId   = tenantId,
                    EntityId   = primaryKeyValue!.ToString(),
                    EntityType = MultiTenantConfiguration.GetEntityType(entityType)
                });

                tenantMappings.AddRange(mappings);
            }


            if (tenantMappings.Any())
            {
                //// enqueue in a separate task to not block the current thread
                //var methodInfo = this.GetMethodWithName(nameof(AssignTenantMapping));

                //var scheduler = serviceProvider.GetService<IScheduler>();
                //scheduler.ScheduleTaskOnce(methodInfo, tenantMappings);
                try
                {

                    await AssignTenantMapping(tenantMappings);
                }
                catch (Exception exception)
                {

                    throw;
                }
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }



    private async Task AssignTenantMapping(IEnumerable<TenantEntityMapping> tenantMappingEntries)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            using (var dbContext = scope.ServiceProvider.GetService<DbContext>())
            {
                await dbContext.Set<TenantEntityMapping>()
                               .AddRangeAsync(tenantMappingEntries);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}