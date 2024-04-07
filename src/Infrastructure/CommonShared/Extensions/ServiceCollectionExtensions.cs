using AspNet.Extensions.SelfDocumentedProblemResult.ExceptionHandlers;
using DotNetBrightener.Caching.Memory;
using DotNetBrightener.CryptoEngine.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using WebApp.CommonShared;
using WebApp.CommonShared.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the required services for the web application to the specified <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/></param>
    /// <param name="configuration">The <see cref="IConfiguration"/></param>
    /// <returns>
    ///     The same instance of this <see cref="IServiceCollection"/> for chaining operations
    /// </returns>
    public static IServiceCollection AddCommonWebAppServices(this IServiceCollection serviceCollection,
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

        serviceCollection.EnableBackgroundTaskServices(configuration);

        serviceCollection.AddExceptionHandler<UnhandledExceptionResponseHandler>();

        serviceCollection.AddProblemDetails();

        return serviceCollection;
    }

    /// <summary>
    ///     Adds the required middlewares for the web application to the specified <see cref="IApplicationBuilder"/>
    /// </summary>
    /// <remarks>
    ///     The pre-configured middlewares are<br />
    ///     - ForwardedHeaders<br />
    ///     - HttpsRedirection<br />
    ///     - ExceptionHandler<br />
    ///
    ///     This method should be called before all other middlewares in the pipeline
    /// </remarks>
    /// <param name="app">The <see cref="IApplicationBuilder"/></param>
    /// <returns>
    ///     The same instance of this <see cref="IApplicationBuilder"/> for chaining operations
    /// </returns>
    public static IApplicationBuilder UseCommonWebAppServices(this IApplicationBuilder app)
    {
        app.UseForwardedHeaders();

        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
        {
            app.UseHttpsRedirection();
        }

        app.UseExceptionHandler();

        return app;
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