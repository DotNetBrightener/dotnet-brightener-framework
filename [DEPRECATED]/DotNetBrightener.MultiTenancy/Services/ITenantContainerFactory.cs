using System;
using DotNetBrightener.MultiTenancy.Configurations;
using DotNetBrightener.MultiTenancy.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.MultiTenancy.Services
{
    public interface ITenantContainerFactory
    {
        TenantContainer CreateContainer(ITenant tenant);
    }

    public class TenantContainerFactory : ITenantContainerFactory
    {
        private readonly IServiceProvider   _serviceProvider;
        private readonly IServiceCollection _applicationServices;
        private readonly ILogger            _logger;

        public TenantContainerFactory(IServiceProvider                serviceProvider,
                                      IServiceCollection              applicationServices,
                                      ILogger<TenantContainerFactory> logger)
        {
            _applicationServices = applicationServices;
            _serviceProvider     = serviceProvider;
            _logger              = logger;
        }

        public TenantContainer CreateContainer(ITenant tenant)
        {
            var tenantContainer         = new TenantContainer(tenant);

            var tenantServiceCollection = _applicationServices.CreateChildContainer(_serviceProvider);

            tenantServiceCollection.AddSingleton(tenant);
            tenantServiceCollection.AddSingleton(tenantContainer);
            tenantServiceCollection.TryAddScoped<ITenantStartupConfiguration, DefaultTenantStartupConfiguration>();
            
            MultiTenantConfigurator.ConfigureTenant?.Invoke(tenant, tenantServiceCollection);

            tenantContainer.ForegroundServiceProvider = tenantServiceCollection.BuildServiceProvider();
            tenantContainer.BackgroundServiceProvider = tenantServiceCollection.CreateChildContainer()
                                                                               .BuildServiceProvider();

            return tenantContainer;
        }
    }
}