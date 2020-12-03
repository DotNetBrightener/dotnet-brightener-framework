using System;
using System.Collections.Generic;
using DotNetBrightener.Core.ApplicationShell;
using DotNetBrightener.Core.Caching;
using DotNetBrightener.Core.Encryption;
using DotNetBrightener.Core.Events;
using DotNetBrightener.Core.Exceptions;
using DotNetBrightener.Core.IO;
using DotNetBrightener.Core.Permissions;
using DotNetBrightener.Core.RemoteServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.Core
{
    public static class DotNetBrightenerServicesRegistration
    {
        /// <summary>
        ///		Adds default services to the service collection
        /// </summary>
        public static IServiceCollection AddAppDefaultServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            serviceCollection.TryAddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

            serviceCollection.AddLogging();
            serviceCollection.AddOptions();

            serviceCollection.AddSingleton(typeof(IConfigurationFilesProvider), ConfigurationFileProviderFactory);

            return serviceCollection;
        }

        /// <summary>
        ///     Detects and registers the permissions to the system
        /// </summary>
        /// <param name="serviceCollection">
        ///     The <see cref="IServiceCollection"/>
        /// </param>
        /// <param name="serviceTypes">
        ///     If specified, only finds and registers the types detected from the given collection.
        ///     Otherwise, detects and registers from all assemblies loaded into the application.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddSecurityAndPermissions(this IServiceCollection serviceCollection,
                                                                   IEnumerable<Type>       serviceTypes = null)
        {
            // permissions and securities
            serviceCollection.AddSingleton<IPermissionsContainer, PermissionsContainer>();

            if (serviceTypes == null)
            {
                var appAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                serviceCollection.RegisterServiceImplementations<IPermissionProvider>(appAssemblies,
                                                                                      ServiceLifetime.Singleton);
            }
            else
            {
                serviceCollection.RegisterServiceImplementations<IPermissionProvider>(serviceTypes,
                                                                                      ServiceLifetime.Singleton);
            }
            return serviceCollection;
        }

        /// <summary>
        ///     Detects and registers the event publisher service and event handlers to the <see cref="serviceCollection"/>
        /// </summary>
        /// <param name="serviceCollection">
        ///     The <see cref="IServiceCollection"/>
        /// </param>
        /// <param name="serviceTypes">
        ///     If specified, only finds and registers the types detected from the given collection.
        ///     Otherwise, detects and registers from all assemblies loaded into the application.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddEventPublishersAndHandlers(this IServiceCollection serviceCollection,
                                                                       IEnumerable<Type>       serviceTypes = null)
        {
            // Event Pub/Sub
            serviceCollection.AddScoped<IEventPublisher, EventPublisher>();

            if (serviceTypes == null)
            {
                var appAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                serviceCollection.RegisterServiceImplementations<IEventHandler>(appAssemblies,
                                                                                ServiceLifetime.Scoped);
            }
            else
            {
                serviceCollection.RegisterServiceImplementations<IEventHandler>(serviceTypes,
                                                                                ServiceLifetime.Scoped);
            }
            return serviceCollection;
        }

        public static IServiceCollection AddDotNetBrightenerCoreServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IMemoryCache, MemoryCache>();

            serviceCollection.AddSingleton<IAppHostContext, AppHostContext>();
            serviceCollection.AddScoped<IRequestWorkContext, RequestWorkContext>();
            serviceCollection.AddScoped<IPasswordValidationProvider, DefaultPasswordValidationProvider>();
            serviceCollection.AddScoped<ICryptoEngine, CryptoEngine>();

            // supporting cache
            serviceCollection.AddSingleton<IStaticCacheManager, MemoryCacheManager>();

            serviceCollection.AddSingleton<IBackgroundServiceProvider>(provider => new BackgroundServiceProvider(provider));

            serviceCollection.AddScoped<IErrorResultFactory, ErrorResultFactory>();

            // remote service
            serviceCollection.AddScoped<IRestClientService, DefaultRestClientService>();

            // File access
            serviceCollection.AddSingleton<IUploadSystemFileProvider, DefaultUploadSystemFileProvider>();
            serviceCollection.AddSingleton<IRootSystemFileProvider, DefaultRootSystemFileProvider>();

            return serviceCollection;
        }

        private static IConfigurationFilesProvider ConfigurationFileProviderFactory(IServiceProvider provider)
        {
            var hostingEnv = provider.GetService<IWebHostEnvironment>();

            return DefaultConfigurationFilesProvider.Init(hostingEnv);
        }

        public static void LoadAndValidatePermissions(this IApplicationBuilder appBuilder)
        {
            using (var scope = appBuilder.ApplicationServices.CreateScope())
            {
                var permissionContainer = scope.ServiceProvider.GetService<IPermissionsContainer>();

                permissionContainer.LoadAndValidatePermissions();
            }
        }
    }
}