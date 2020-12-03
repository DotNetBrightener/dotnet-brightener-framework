using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetBrightener.Core;
using DotNetBrightener.Core.Localization.Services;
using DotNetBrightener.Core.Modular;
using DotNetBrightener.Core.Modular.Extensions;
using DotNetBrightener.Core.Modular.StartupConfiguration;
using DotNetBrightener.Core.Routing;
using DotNetBrightener.Integration.Modular.Localization;
using DotNetBrightener.Integration.Modular.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace DotNetBrightener.Integration.Modular.Extensions
{
    public static class ModularConfigServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the integration with modular system to the <see cref="IServiceCollection"/>
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="enabledModules">The module ids that are enabled. Leave null to register all detected modules</param>
        /// <returns>
        ///     The loaded module entries
        /// </returns>
        public static LoadedModuleEntries AddModularIntegration(this IServiceCollection serviceCollection,
                                                                string[] enabledModules = null)
        {
            var loadedModuleEntries = serviceCollection.EnableModules(enabledModules);

            serviceCollection.Replace(ServiceDescriptor.Singleton<ILocalizationFileLoader, ModuleLocalizationFileLoader>());
            serviceCollection.TryAddEnumerable(
                              ServiceDescriptor.Transient<IApplicationModelProvider, ModularApplicationModelProvider>()
                             );

            var serviceTypes = loadedModuleEntries.GetExportedTypes();

            serviceCollection.RegisterServiceImplementations<IActionFilterProvider>(serviceTypes);
            serviceCollection.RegisterServiceImplementations<IRoutingConfiguration>(serviceTypes);

            var mvcBuilder = serviceCollection.AddControllersWithViews(options =>
            {
                options.EnableEndpointRouting = false;
                ConfigureMvcFilters(options, loadedModuleEntries);
            });

            foreach (var assembly in loadedModuleEntries.GetModuleAssemblies())
            {
                mvcBuilder.AddApplicationPart(assembly);
            }

            mvcBuilder.AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            return loadedModuleEntries;
        }

        /// <summary>
        ///     Adds support for modular routing with the MVC to the <see cref="IApplicationBuilder"/> request execution pipeline
        /// </summary>
        /// <param name="builder"></param>
        public static void UseModularMvcApplicationRouting(this IApplicationBuilder builder)
        {
            var serviceProvider = builder.ApplicationServices;

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

        /// <summary>
        ///     Detects the Statup classes in given modules, then automatically calls the <see cref="ConfigureService"/> method to
        ///     register the modules' services into the <see cref="services"/>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to register services into</param>
        /// <param name="loadedModuleEntries">
        ///     The loaded module entries
        /// </param>
        /// <remarks>
        ///     The purpose of this configuration method is we don't need to force the module to conform to the framework.
        /// </remarks>
        /// <returns>
        ///     A collection of service types
        /// </returns>
        public static IEnumerable<Type> ConfigureServicesFromModules(this IServiceCollection services,
                                                                     LoadedModuleEntries     loadedModuleEntries)
        {
            var otherModuleEntries =
                new LoadedModuleEntries(loadedModuleEntries.Where(_ => _.ModuleId !=
                                                                       ModuleEntry.MainModuleIdentifier));

            var loadedTypes = loadedModuleEntries.GetExportedTypes();

            services.RegisterServiceImplementations<IModuleStartupConfiguration>(loadedTypes);

            var moduleLoadedTypes = new ModuleExportedTypeCollection(loadedTypes);
            services.AddSingleton<List<Type>>(moduleLoadedTypes);

            var startupTypes = otherModuleEntries.GetExportedTypesWithName("Startup")
                                                 .OrderByModuleDependencies(otherModuleEntries)
                                                 .ToArray();

            if (startupTypes.Length == 0)
                return loadedTypes;

            RegisterStartupTypes(services, loadedModuleEntries, startupTypes);

            using (var serviceProvider = services.BuildServiceProvider())
            {
                foreach (var startupType in startupTypes)
                {
                    // get the ConfigureServices(IServiceCollection services) method from the 'Startup' file
                    var configureServicesMethod = startupType.GetMethods()
                                                             .Where(_ => _.Name == "ConfigureServices")
                                                             .FirstOrDefault(_ => _.GetParameters().Length == 1 &&
                                                                                  _.GetParameters()[0].ParameterType ==
                                                                                  typeof(IServiceCollection));

                    if (configureServicesMethod == null)
                        continue;

                    var startupInstance = serviceProvider.GetService(startupType);

                    if (startupInstance == null)
                        continue;

                    configureServicesMethod.Invoke(startupInstance,
                                                   new object[]
                                                   {
                                                       services
                                                   });
                }
            }

            return loadedTypes;
        }

        public static Task ExecuteStartupTasks(this IApplicationBuilder appBuilder)
        {
            return ExecuteTaskFromModulesStartup(appBuilder, "OnAppStartup", ExecuteMethodFromStartUpTask);
        }

        private static async Task ExecuteTaskFromModulesStartup(this IApplicationBuilder appBuilder, 
                                                               string executeMethod,
                                                               Func<string, Type, IServiceProvider, Task> action)
        {
            using (var serviceProviderScope = appBuilder.ApplicationServices.CreateScope())
            {
                var serviceProvider = serviceProviderScope.ServiceProvider;
                {
                    var cachedStartupTypes = serviceProvider.GetService<StartupClassCollection>();
                    if (cachedStartupTypes == null)
                        throw new InvalidOperationException($"Cannot find the startup files collection.");

                    var tasks = new List<Task>();

                    foreach (var startupType in cachedStartupTypes)
                    {
                        tasks.Add(action.Invoke(executeMethod, startupType, serviceProvider));
                    }

                    await Task.WhenAll(tasks);
                }
            }
        }

        private static async Task ExecuteMethodFromStartUpTask(string           methodNameToExecute,
                                                               Type             startupType,
                                                               IServiceProvider serviceProvider)
        {
            var _methodInfo = startupType
                             .GetMethods()
                             .FirstOrDefault(_ => _.Name == methodNameToExecute);

            if (_methodInfo == null)
                return;

            var startupInstance = serviceProvider.GetService(startupType);

            if (startupInstance == null)
                return;

            var parameters = _methodInfo.GetParameters()
                                        .Select(_ => serviceProvider.GetService(_.ParameterType))
                                        .ToArray();

            var isAwaitable = _methodInfo.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;

            if (isAwaitable)
            {
                if (_methodInfo.ReturnType.IsGenericType)
                {
                    await (dynamic) _methodInfo.Invoke(startupInstance, parameters);
                }
                else
                {
                    await (Task) _methodInfo.Invoke(startupInstance, parameters);
                }
            }
            else
            {
                _methodInfo.Invoke(startupInstance, parameters);
            }
        }

        private static void RegisterStartupTypes(IServiceCollection  services,
                                                 LoadedModuleEntries loadedModuleEntries,
                                                 Type[]              startupTypes)
        {
            var startupClassCollection = new StartupClassCollection(startupTypes);
            services.AddSingleton(startupClassCollection);

            foreach (var startupType in startupTypes)
            {
                var associatedModule = loadedModuleEntries.GetAssociatedModuleEntry(startupType);
                if (associatedModule.Configuration == null)
                {
                    services.AddScoped(startupType);
                    continue;
                }

                bool registered   = false;
                var  constructors = startupType.GetConstructors();
                foreach (var constructorInfo in constructors)
                {
                    var constructorParams = constructorInfo.GetParameters();
                    if (constructorParams.Length == 1 &&
                        constructorParams[0].ParameterType == typeof(IConfiguration))
                    {
                        services.AddScoped(startupType,
                                           provider => constructorInfo.Invoke(new[]
                                           {
                                               associatedModule.Configuration
                                           }));
                        registered = true;
                        break;
                    }
                }

                if (!registered)
                    services.AddScoped(startupType);
            }
        }


        private static void ConfigureMvcFilters(MvcOptions options, LoadedModuleEntries loadedModules)
        {
            var actionFilterTypes = loadedModules.GetExportedTypesOfType<IActionFilterProvider>();

            foreach (var actionFilterProviderType in actionFilterTypes)
            {
                options.Filters.Add(actionFilterProviderType);
            }
        }

        private class StartupClassCollection : List<Type>
        {
            public StartupClassCollection(IEnumerable<Type> collection) : base(collection)
            {

            }
        }

        private class ModuleExportedTypeCollection : List<Type>
        {
            public ModuleExportedTypeCollection(IEnumerable<Type> collection) : base(collection)
            {

            }
        }
    }
}