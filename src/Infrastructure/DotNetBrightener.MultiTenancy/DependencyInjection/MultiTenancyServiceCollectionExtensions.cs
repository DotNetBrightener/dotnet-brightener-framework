using DotNetBrightener.DataAccess.EF.Events;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.MultiTenancy;
using DotNetBrightener.MultiTenancy.Events;
using DotNetBrightener.MultiTenancy.Permissions;
using DotNetBrightener.MultiTenancy.Services;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class MultiTenancyServiceCollectionExtensions
{
    public static MultiTenantConfiguration EnableMultiTenancy(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ITenantAccessor, TenantAccessor>();
        serviceCollection.AddScoped<ITenantEntityMappingService, TenantEntityMappingService>();
        serviceCollection.AddScoped<ITenantDataService, TenantDataService>();

        serviceCollection.RegisterPermissionProvider<TenantManagementPermissions>();

        serviceCollection.Replace(ServiceDescriptor.Scoped<IRepository, TenantSupportedRepository>());

        serviceCollection.AddScoped<
            IEventHandler<DbContextAfterSaveChanges>,
            DbContextAfterSaveChanges_StoreTenantMapping
        >();

        serviceCollection.AddScoped<
            IEventHandler,
            DbContextAfterSaveChanges_StoreTenantMapping
        >();

        var multiTenantConfiguration = new MultiTenantConfiguration();
        serviceCollection.AddSingleton(multiTenantConfiguration);

        return multiTenantConfiguration;
    }

    public static MultiTenantConfiguration RegisterTenantMappableTypes(this   MultiTenantConfiguration config,
                                                                       params Type[]                   types)
    {
        foreach (var type in types)
        {
            config.RegisterTenantMappableType(type);
        }

        return config;
    }

    /// <summary>
    ///     Enables middleware for detection of tenants.
    ///     This should be called after authentication middleware
    /// </summary>
    /// <param name="app"></param>
    public static void UseTenantDetection(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantDetectionAndCorsEnableMiddleware>();
        app.UseMiddleware<TenantDetectionMiddleware>();
    }
}
