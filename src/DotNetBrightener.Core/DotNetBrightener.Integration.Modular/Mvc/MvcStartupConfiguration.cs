using System;
using System.Linq;
using DotNetBrightener.Core;
using DotNetBrightener.Core.Localization.Services;
using DotNetBrightener.Core.Modular;
using DotNetBrightener.Core.Modular.StartupConfiguration;
using DotNetBrightener.Core.Routing;
using DotNetBrightener.Integration.Modular.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace DotNetBrightener.Integration.Modular.Mvc
{
    public class MvcStartupConfiguration : IStartupConfiguration
    {
        private readonly LoadedModuleEntries _loadedModuleEntries;

        /// <summary>
        ///     This Order is to make sure the MVC configuration will be run first
        /// </summary>
        public int Order => -100;

        public MvcStartupConfiguration(LoadedModuleEntries loadedModuleEntries)
        {
            _loadedModuleEntries = loadedModuleEntries;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var serviceTypes = _loadedModuleEntries.GetExportedTypes();

            services.Replace(ServiceDescriptor.Singleton<ILocalizationFileLoader, ModuleLocalizationFileLoader>());
            services.RegisterServiceImplementations<IActionFilterProvider>(serviceTypes);
            services.RegisterServiceImplementations<IRoutingConfiguration>(serviceTypes);

            services.TryAddEnumerable(ServiceDescriptor
                                         .Transient<IApplicationModelProvider, ModularApplicationModelProvider>());

            var mvcBuilder = services.AddControllersWithViews(options =>
                                                              {
                                                                  options.EnableEndpointRouting = false;
                                                                  ConfigureMvcFilters(options, _loadedModuleEntries);
                                                              });

            foreach (var assembly in _loadedModuleEntries.GetModuleAssemblies())
            {
                mvcBuilder.AddApplicationPart(assembly);
            }

            mvcBuilder.AddNewtonsoftJson(options =>
                                         {
                                             options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                                         });
        }

        public void Configure(IApplicationBuilder builder, IServiceProvider serviceProvider)
        {
            builder.UseRequestLocalization();
            builder.UseRouting();

            builder.UseMvc(options =>
                           {
                               using (var scope = serviceProvider.CreateScope())
                               {
                                   var routingConfigurations = scope.ServiceProvider
                                                                    .GetServices<IRoutingConfiguration>()
                                                                    .ToArray();

                                   if (routingConfigurations.Any())
                                   {
                                       foreach (var routingConfiguration in routingConfigurations)
                                       {
                                           var router = routingConfiguration.ConfigureRoute(serviceProvider);
                                           options.Routes.Insert(routingConfiguration.Order, router);
                                       }
                                   }
                               }

                               options.MapRoute("default",
                                                "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                           });
        }

        private void ConfigureMvcFilters(MvcOptions options, LoadedModuleEntries loadedModules)
        {
            var actionFilterTypes = loadedModules.GetExportedTypesOfType<IActionFilterProvider>();

            foreach (var actionFilterProviderType in actionFilterTypes)
            {
                options.Filters.Add(actionFilterProviderType);
            }
        }
    }
}