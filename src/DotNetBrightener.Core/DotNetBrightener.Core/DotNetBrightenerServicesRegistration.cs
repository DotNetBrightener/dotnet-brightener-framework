using DotNetBrightener.Core.ApplicationShell;
using DotNetBrightener.Core.Caching;
using DotNetBrightener.Core.Encryption;
using DotNetBrightener.Core.Exceptions;
using DotNetBrightener.Core.IO;
using DotNetBrightener.Core.Mvc;
using DotNetBrightener.Core.RemoteServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

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

        public static IServiceCollection AddDotNetBrightenerCoreServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IMemoryCache, MemoryCache>();

            serviceCollection.AddSingleton<IAppHostContext, AppHostContext>();
            serviceCollection.AddScoped<IRequestWorkContext, RequestWorkContext>();
            serviceCollection.AddScoped<IPasswordValidationProvider, DefaultPasswordValidationProvider>();
            serviceCollection.AddScoped<ICryptoEngine, CryptoEngine>();

            // supporting cache
            serviceCollection.AddSingleton<IStaticCacheManager, MemoryCacheManager>();

            serviceCollection.AddScoped<IUnhandleExceptionHandler, DefaultUnhandledExceptionHandler>();
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
    }
}