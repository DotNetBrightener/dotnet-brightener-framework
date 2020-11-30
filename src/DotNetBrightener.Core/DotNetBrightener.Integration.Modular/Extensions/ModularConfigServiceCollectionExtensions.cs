using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetBrightener.Core.Modular;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Integration.Modular.Extensions
{
    public static class ModularConfigServiceCollectionExtensions
    {
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

        /// <summary>
        ///     Automatically calls the <see cref="ConfigureService"/> method of the Startup classes detected from the loaded modules
        /// </summary>
        /// <param name="services"></param>
        /// <param name="loadedModuleEntries">The loaded module entries</param>
        /// <remarks>
        ///     The purpose of this configuration method is we don't need to force the module to conform to the framework.
        ///     
        /// </remarks>
        public static void ConfigureModuleServices(this IServiceCollection services,
                                                   LoadedModuleEntries     loadedModuleEntries)
        {
            var otherModuleEntries =
                new LoadedModuleEntries(loadedModuleEntries.Where(_ => _.ModuleId !=
                                                                       ModuleEntry.MainModuleIdentifier));


            var loadedTypes = loadedModuleEntries.GetExportedTypes().Where(_ => _.IsNotSystemType());

            var moduleLoadedTypes = new ModuleExportedTypeCollection(loadedTypes);
            services.AddSingleton<List<Type>>(moduleLoadedTypes);

            var startupTypes = otherModuleEntries.GetExportedTypesWithName("Startup")
                                                 .OrderByModuleDependencies(otherModuleEntries)
                                                 .ToArray();

            if (startupTypes.Length == 0)
                return;

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
        }

        public static Task ExecuteStartupTasks(this IApplicationBuilder appBuilder)
        {
            return ExecuteTaskFromModulesStartup(appBuilder, "OnAppStartup", ExecuteMethodFromStartUpTask);
        }

        public static async Task ExecuteTaskFromModulesStartup(this IApplicationBuilder appBuilder, 
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
    }
}