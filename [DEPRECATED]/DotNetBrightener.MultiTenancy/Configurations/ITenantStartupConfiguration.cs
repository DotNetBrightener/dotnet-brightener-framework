using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.MultiTenancy.Configurations
{
    /// <summary>
    ///     Represents the service to configure the <see cref="IApplicationBuilder"/> which represents the tenant pipeline
    /// </summary>
    public interface ITenantStartupConfiguration
    {
        Task ConfigureServices(IServiceCollection serviceCollection);

        Task ConfigureAsync(IApplicationBuilder tenantAppBuilder);
    }

    internal class DefaultTenantStartupConfiguration : ITenantStartupConfiguration
    {
        public virtual Task ConfigureServices(IServiceCollection serviceCollection)
        {
            return Task.CompletedTask;
        }

        public virtual Task ConfigureAsync(IApplicationBuilder tenantAppBuilder)
        {
            return Task.CompletedTask;
        }
    }
}