using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.MultiTenancy.Entities;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using DotNetBrightener.DataAccess.Services;

namespace DotNetBrightener.MultiTenancy.Services;

public class TenantSupportedRepository : Repository
{
    private readonly ITenantAccessor                    _tenantAccessor;
    private readonly ILogger<TenantSupportedRepository> _logger;

    /// <summary>
    ///     Identify if current module has tenant mapping table or not
    /// </summary>
    internal static bool? HasTenantMapping;

    private static readonly object LockObj = new();

    public TenantSupportedRepository(DbContext                          dbContext,
                                     ICurrentLoggedInUserResolver       currentLoggedInUserResolver,
                                     IEventPublisher                    eventPublisher,
                                     ITenantAccessor                    tenantAccessor,
                                     IServiceProvider                   backgroundServiceProvider,
                                     IAuditingContainer                 auditingContainer,
                                     ILogger<TenantSupportedRepository> logger)
        : base(dbContext, currentLoggedInUserResolver, eventPublisher, auditingContainer)
    {
        _tenantAccessor = tenantAccessor;
        _logger         = logger;

        if (HasTenantMapping is null)
        {
            lock (LockObj)
                CheckTenantMappingTable(backgroundServiceProvider, dbContext.GetType());
        }
    }

    public override IQueryable<T> Fetch<T>(Expression<Func<T, bool>> expression = null)
    {
        // override the logic of loading a record from database for specifies tenant
        var query            = base.Fetch(expression);
        var currentTenantIds = _tenantAccessor.CurrentTenant?.Id;

        // if we don't need to load tenant mapping (because the entity does not support),
        // or current tenant not specified (because user has access to ALL tenant or user not logged in)
        // then just use the original method
        if (HasTenantMapping == false ||
            MultiTenantConfiguration.ShouldIgnoreTenantMapping<T>() ||
            !currentTenantIds.HasValue
           )
        {
            _logger.LogDebug($"No multi-tenant mapping support for entity of type {typeof(T).FullName}");

            return query;
        }

        var currentTenantIdValue = currentTenantIds.Value;
        var entityTypeName       = MultiTenantConfiguration.GetEntityType<T>();

        var tenantMappingQuery = DbContext.Set<TenantEntityMapping>()
                                          .Where(_ => _.EntityType == entityTypeName);

        var joinedResult = query.GroupJoin(tenantMappingQuery,
                                           ExpressionExtensions
                                              .BuildMemberAccessExpression<T>(nameof(BaseEntity.Id)),
                                           _ => _.EntityId,
                                           (entity, mapping) => new
                                           {
                                               entity,
                                               mapping
                                           })
                                .SelectMany(x => x.mapping.DefaultIfEmpty(),
                                            (grouped, tenantEntityMapping) => new
                                            {
                                                grouped.entity,
                                                tenantEntityMapping
                                            })
                                .Where(joined => joined.tenantEntityMapping == null ||
                                                 currentTenantIdValue == joined.tenantEntityMapping.TenantId)
                                .Select(joined => joined.entity);

        return joinedResult;
    }

    private static void CheckTenantMappingTable(IServiceProvider backgroundServiceProvider, Type dbContextType)
    {
        using var scope = backgroundServiceProvider.CreateScope();

        var serviceType = scope.ServiceProvider.GetService(dbContextType);

        try
        {
            if (serviceType is not DbContext dbContext)
                return;

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            dbContext!.Set<TenantEntityMapping>().Count();

            HasTenantMapping = true;
        }
        catch
        {
            HasTenantMapping = false;
        }
    }
}