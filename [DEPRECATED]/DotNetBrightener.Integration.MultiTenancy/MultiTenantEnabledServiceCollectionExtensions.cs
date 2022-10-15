using System;
using System.Collections.Generic;
using System.Linq;
using DotNetBrightener.Core.Authentication.Configs;
using DotNetBrightener.Core.Authentication.Services;
using DotNetBrightener.Core.IO;
using DotNetBrightener.MultiTenancy;
using DotNetBrightener.MultiTenancy.StartUps;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.Integration.MultiTenancy
{
    public static class MultiTenantEnabledServiceCollectionExtensions
    {
        public static void AddMultiTenantIntegration(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ITenantStartupTask, TenantStartupTask>();

            new List<ServiceDescriptor>
                {
                    ServiceDescriptor.Singleton<IConfigurationFilesProvider>(ConfigurationFilesProviderFactory),

                    ServiceDescriptor
                       .Scoped<IJwtSecurityKeySigningResolver, TenantBasedJwtSecurityKeySigningResolver>(),

                    ServiceDescriptor.Singleton<IJwtConfigurationAccessor, TenantBasedJwtConfigurationAccessor>()
                }
               .ForEach(_ => serviceCollection.Replace(_));
            
            var jwtConfigServiceDescriptor = serviceCollection.FirstOrDefault(_ => _.ServiceType == typeof(JwtConfig));
            serviceCollection.Remove(jwtConfigServiceDescriptor);

            serviceCollection.AddScoped<JwtConfig>(JwtConfigFactory);
        }

        private static IConfigurationFilesProvider ConfigurationFilesProviderFactory(IServiceProvider provider)
        {
            var hostEnvironment = provider.GetService<IWebHostEnvironment>();
            var tenantSetting   = provider.GetService<ITenant>();

            return TenantBaseConfigurationFilesProvider.InitializeProvider(hostEnvironment, tenantSetting);
        }

        private static JwtConfig JwtConfigFactory(IServiceProvider provider)
        {
            var tenantSetting     = provider.GetService<ITenant>();
            var jwtConfigAccessor = provider.GetService<IJwtConfigurationAccessor>();

            return jwtConfigAccessor.RetrieveConfig(tenantSetting.TenantGuid);
        }
    }
}