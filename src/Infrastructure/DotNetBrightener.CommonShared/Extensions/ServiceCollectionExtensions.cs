using DotNetBrightener.CommonShared.BackgroundTasks;
using DotNetBrightener.CommonShared.Mvc;
using DotNetBrightener.CommonShared.Services;
using DotNetBrightener.CommonShared.StartupTasks;
using DotNetBrightener.Core.BackgroundTasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using System.Net;
using System.Reflection;

namespace DotNetBrightener.CommonShared.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection serviceCollection,
                                         IConfiguration          configuration)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        serviceCollection.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        serviceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        serviceCollection.TryAddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

        serviceCollection.AddOptions();
        serviceCollection.AddLocalization();
        serviceCollection.AddSystemDateTimeProvider();

        serviceCollection.AddScoped<IErrorResultFactory, ErrorResultFactory>();

        // Logging Support
        serviceCollection.RegisterLoggingService<IBackgroundServiceProvider>();
        serviceCollection.AddBackgroundTask<QueueEventLogBackgroundTask>();

        serviceCollection.Configure<CoreSettings>(configuration);
        serviceCollection.RegisterStartupTask<BackgroundTaskEnableStartupTask>();
    }

    public static void RegisterExceptionHandler<T>(this IServiceCollection serviceCollection)
        where T : class, IUnhandledExceptionHandler
    {
        serviceCollection.AddScoped<IUnhandledExceptionHandler, T>();
    }

    public static void AutoRegisterDependencyServices(this IServiceCollection serviceCollection,
                                                      Assembly[]              loadedAssemblies)
    {
        var dependencyTypes = loadedAssemblies.GetDerivedTypes<IDependency>();

        foreach (var dependencyType in dependencyTypes)
        {
            var interfaces = dependencyType.GetInterfaces().ToArray();

            // Get only direct parent interfaces
            interfaces = interfaces.Except(interfaces.SelectMany(t => t.GetInterfaces()))
                                   .ToArray();

            foreach (var @interface in interfaces)
            {
                var lifetime = ServiceLifetime.Scoped;

                if (@interface.IsAssignableTo(typeof(ISingletonDependency)))
                {
                    lifetime = ServiceLifetime.Singleton;
                }
                else if (@interface.IsAssignableTo(typeof(ITransientDependency)))
                {
                    lifetime = ServiceLifetime.Transient;
                }

                if (@interface.IsGenericType &&
                    dependencyType.IsGenericType)
                {
                    var interfaceGenericTypeDef  = @interface.GetGenericTypeDefinition();
                    var dependencyGenericTypeDef = dependencyType.GetGenericTypeDefinition();

                    serviceCollection.Add(ServiceDescriptor.Describe(interfaceGenericTypeDef,
                                                                     dependencyGenericTypeDef,
                                                                     lifetime));
                    // register the type itself
                    serviceCollection.TryAdd(ServiceDescriptor.Describe(dependencyGenericTypeDef,
                                                                        dependencyGenericTypeDef,
                                                                        lifetime));
                }
                else
                {
                    var existingRegistration = serviceCollection.FirstOrDefault(_ => _.ServiceType == @interface &&
                                                                                     _.ImplementationType ==
                                                                                     dependencyType);

                    if (existingRegistration != null)
                        continue;

                    serviceCollection.Add(ServiceDescriptor.Describe(@interface,
                                                                     dependencyType,
                                                                     lifetime));
                    // register the type itself
                    serviceCollection.TryAdd(ServiceDescriptor.Describe(dependencyType,
                                                                        dependencyType,
                                                                        lifetime));
                }
            }
        }
    }

    public static void RegisterFilterProvider<T>(this IServiceCollection serviceCollection)
        where T : class, IActionFilterProvider
    {
        serviceCollection.AddScoped<IActionFilterProvider, T>();
    }
}