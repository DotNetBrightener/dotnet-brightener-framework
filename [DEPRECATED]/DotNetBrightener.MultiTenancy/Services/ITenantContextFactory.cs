using DotNetBrightener.MultiTenancy.Contexts;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.MultiTenancy.Services
{
    /// <summary>
    /// The service that create the tenant context by coordinating all the components from given <see cref="TenantSetting"/>
    /// </summary>
    public interface ITenantContextFactory
    {
        /// <summary>
        ///		Builds a tenant context from the given tenant setting
        /// </summary>
        TenantContext CreateTenantContext(ITenant setting);
    }

    public class TenantContextFactory : ITenantContextFactory
    {
        private readonly ILogger                 _logger;
        private readonly ITenantContainerFactory _tenantContainerFactory;

        public TenantContextFactory(ILogger<TenantContextFactory> logger,
                                    ITenantContainerFactory       tenantContainerFactory)
        {
            _logger                 = logger;
            _tenantContainerFactory = tenantContainerFactory;
        }

        public TenantContext CreateTenantContext(ITenant setting)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"Creating context for tenant '{setting.Name}'");
            }

            var tenantContainer = _tenantContainerFactory.CreateContainer(setting);

            return new TenantContext(setting, tenantContainer);
        }
    }
}