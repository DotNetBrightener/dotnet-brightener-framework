using DotNetBrightener.DataAccess.EF.Internal;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.MultiTenancy;
using DotNetBrightener.MultiTenancy.DbContexts;
using DotNetBrightener.MultiTenancy.Entities;
using DotNetBrightener.MultiTenancy.Permissions;
using DotNetBrightener.MultiTenancy.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class MultiTenancyServiceCollectionExtensions
{
    public static MultiTenantConfiguration EnableMultiTenancy<TTenant>(this IServiceCollection serviceCollection)
        where TTenant : TenantBase, new()
    {
        serviceCollection.TryAddSingleton<EFCoreExtendedServiceFactory>();
        serviceCollection.AddHttpContextAccessor();

        serviceCollection.AddScoped<ITenantAccessor, TenantAccessor>();
        serviceCollection.AddScoped<ITenantEntityMappingService, TenantEntityMappingService>();
        
        serviceCollection.AddScoped(typeof(ITenantDataService<TTenant>),
                                    typeof(TenantDataService<TTenant>));

        serviceCollection.RegisterPermissionProvider<TenantManagementPermissions>();

        serviceCollection.Replace(ServiceDescriptor.Scoped<IRepository, TenantSupportedRepository>());


        serviceCollection.AddDbContextConfigurator<MultiTenantEnabledDbContextConfigurator>();
        serviceCollection.TryAddScoped<MultiTenantEnabledSavingChangesInterceptor>();
        serviceCollection.AddScoped<IInterceptorsEntriesContainer, InterceptorEntriesContainer>();

        var multiTenantConfiguration = new MultiTenantConfiguration(serviceCollection);
        serviceCollection.AddSingleton(multiTenantConfiguration);

        return multiTenantConfiguration;
    }

    /// <summary>
    ///     Enables middleware for detection of tenants.
    ///     This should be called after authentication and authorization middleware
    /// </summary>
    /// <param name="app"></param>
    public static void UseTenantDetection<TTenant>(this IApplicationBuilder app) where TTenant : TenantBase, new()
    {
        app.UseMiddleware<TenantDetectionMiddleware<TTenant>>();
    }
}