using System;
using DotNetBrightener.MultiTenancy.MiddleWares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace DotNetBrightener.MultiTenancy
{
    public class MultiTenantConfigurationBuilder
    {
        public Action<IApplicationBuilder, IWebHostEnvironment> Configure { get; set; }
    }

    public static class MultiTenantApplicationBuilderExtensions
    {
        /// <summary>
        ///     Indicates that the application uses multi tenant system
        /// </summary>
        /// <param name="appBuilder">
        ///     The <see cref="IApplicationBuilder"/>
        /// </param>
        /// <param name="action">
        ///     The action to configure after the multi-tenant pipeline is configured, such as adding other middlewares etc...
        ///     After this action, the router middleware for multi-tenant (<see cref="MultiTenantRouterMiddleware" />) will be added to the pipeline
        /// </param>
        /// <param name="options"></param>
        public static void UseMultiTenant(this IApplicationBuilder    appBuilder,
                                          Action<IApplicationBuilder, IWebHostEnvironment> options = null,
                                          IWebHostEnvironment environment = null)
        {
            UseMultiTenant(appBuilder, null, builder => builder.Configure = options, environment);
        }

        public static void UseMultiTenant(this IApplicationBuilder    appBuilder,
                                          Action<MultiTenantConfigurationBuilder> options = null,
                                          IWebHostEnvironment environment = null)
        {
            UseMultiTenant(appBuilder, null, options, environment);
        }

        public static void UseMultiTenant(this IApplicationBuilder    appBuilder,
                                          Action<IApplicationBuilder, IWebHostEnvironment> action = null,
                                          Action<MultiTenantConfigurationBuilder> options = null,
                                          IWebHostEnvironment environment = null)
        {
            var tenantAppConfigure = new MultiTenantConfigurationBuilder();
            options?.Invoke(tenantAppConfigure);

            appBuilder.UseMiddleware<TenantPipelineBuilderMiddleware>(tenantAppConfigure);

            action?.Invoke(appBuilder, environment);

            appBuilder.UseMiddleware<TenantRoutingMiddleware>();
        }
    }
}