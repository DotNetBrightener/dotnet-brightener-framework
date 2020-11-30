using System;
using DotNetBrightener.MultiTenancy.MiddleWares;
using DotNetBrightener.MultiTenancy.Services;
using DotNetBrightener.MultiTenancy.StartUps;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.MultiTenancy
{
    public static class MultiTenantEnabledServiceCollectionExtensions
    {
        public static void AddMultiTenantSupport<TTenantEntity>(
            this IServiceCollection                                       serviceCollection,
            Action<TTenantEntity, IServiceCollection> configureTenant = null)
            where TTenantEntity : ITenant
        {
            serviceCollection.AddSingleton<IApplicationHost, ApplicationHost>();
            serviceCollection.AddSingleton<ITenantContextFactory, TenantContextFactory>();
            serviceCollection.AddSingleton<ITenantPipelineContainer, TenantPipelineContainer>();
            serviceCollection.AddSingleton<ITenantContainerFactory, TenantContainerFactory>();
            serviceCollection.AddSingleton<IRunningTenantTable, RunningTenantTable>();
            serviceCollection.AddSingleton<ITenantManager, TenantManager>();
            
            serviceCollection.AddScoped<ITenantStartupTaskExecutor, DefaultTenantStartupTaskExecutor>();
            serviceCollection.AddScoped<ITenantStartupTask, DefaultTenantStartupTask>();

            MultiTenantConfigurator.TenantType = typeof(TTenantEntity);

            if (configureTenant != null)
                MultiTenantConfigurator.ConfigureTenant =
                    (tenantObject, tenantServiceCollection) =>
                    {
                        if (tenantObject is TTenantEntity tenant)
                        {
                            configureTenant.Invoke(tenant, tenantServiceCollection);
                        }
                    };
        }
    }
}