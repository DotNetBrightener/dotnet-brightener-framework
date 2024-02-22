using System.Linq;
using System.Net;
using System.Reflection;
using DotNetBrightener.Caching.Memory;
using DotNetBrightener.CryptoEngine.DependencyInjection;
using DotNetBrightener.WebApp.CommonShared.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;

namespace DotNetBrightener.WebApp.CommonShared.Extensions;

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
        serviceCollection.AddCryptoEngine(configuration);
        
        serviceCollection.EnableCachingService();
        serviceCollection.EnableMemoryCacheService();

        serviceCollection.AddPermissionAuthorization();
        serviceCollection.AddEventPubSubService();

        serviceCollection.EnableBackgroundTaskServices();
        
        serviceCollection.RegisterExceptionHandler<DefaultUnhandledExceptionHandler>();
        serviceCollection.RegisterFilterProvider<UnhandledExceptionResponseHandler>();
    }

    public static IMvcBuilder AddCommonMvcApp(this IServiceCollection serviceCollection,
                                              IConfiguration          configuration = null)
    {
        var mvcBuilder = serviceCollection.AddControllersWithViews(mvcOption =>
                                           {
                                               mvcOption.RegisterFilterProviders(serviceCollection);
                                               mvcOption.ModelBinderProviders
                                                        .Insert(0, new CommaSeparatedArrayModelBinderProvider());
                                           })
                                          .AddRazorRuntimeCompilation()
                                          .AddNewtonsoftJson(config =>
                                           {
                                               config.SerializerSettings.ContractResolver =
                                                   DefaultJsonSerializer.DefaultJsonSerializerSettings
                                                                        .ContractResolver;
                                               config.SerializerSettings.ReferenceLoopHandling =
                                                   DefaultJsonSerializer.DefaultJsonSerializerSettings
                                                                        .ReferenceLoopHandling;
                                               config.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                                           });


        mvcBuilder.AddViewLocalization();
        mvcBuilder.AddDataAnnotationsLocalization();

        return mvcBuilder;
    }

    public static void RegisterExceptionHandler<T>(this IServiceCollection serviceCollection)
        where T : class, IUnhandledExceptionHandler
    {
        serviceCollection.AddScoped<IUnhandledExceptionHandler, T>();
    }

    public static void RegisterFilterProvider<T>(this IServiceCollection serviceCollection)
        where T : class, IActionFilterProvider
    {
        serviceCollection.AddScoped<IActionFilterProvider, T>();
    }


    public static void AutoRegisterDependencyServices(this IServiceCollection serviceCollection,
                                                      Assembly[] loadedAssemblies)
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
                    var interfaceGenericTypeDef = @interface.GetGenericTypeDefinition();
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
}