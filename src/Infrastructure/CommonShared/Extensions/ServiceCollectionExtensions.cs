using AspNet.Extensions.SelfDocumentedProblemResult.ExceptionHandlers;
using DotNetBrightener.Caching.Memory;
using DotNetBrightener.CryptoEngine.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using WebApp.CommonShared;
using WebApp.CommonShared.Endpoints;
using WebApp.CommonShared.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the commonly-used required services for the web application to the specified <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/></param>
    /// <param name="configuration">The <see cref="IConfiguration"/></param>
    /// <returns>
    ///     The same instance of this <see cref="IServiceCollection"/> for chaining operations
    /// </returns>
    public static CommonAppBuilder AddCommonWebAppServices(this IServiceCollection serviceCollection,
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
        var eventPubSubBuilder = serviceCollection.AddEventPubSubService();

        var commonAppBuilder = new CommonAppBuilder
        {
            Services                  = serviceCollection,
            EventPubSubServiceBuilder = eventPubSubBuilder
        };

        serviceCollection.EnableBackgroundTaskServices(configuration);

        serviceCollection.AddExceptionHandler<UnhandledExceptionResponseHandler>();

        serviceCollection.AddProblemDetails();

        serviceCollection.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        serviceCollection.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        serviceCollection.AddSingleton(serviceCollection);
        serviceCollection.AddSingleton(commonAppBuilder);

        return commonAppBuilder;
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

    /// <summary>
    ///     Register old-fashioned MVC services to the specified <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     Registers all the endpoint registrars found in the <see cref="loadedAssemblies"/>
    ///     to the given <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    /// <param name="loadedAssemblies">
    ///     The loaded assemblies
    /// </param>
    public static void AddEndpointRegistrars(this IServiceCollection serviceCollection,
                                             Assembly[]              loadedAssemblies)
    {
        var endpointRegistrarTypes = loadedAssemblies.GetDerivedTypes<IEndpointRegistrar>();

        foreach (var endpointRegistrarType in endpointRegistrarTypes)
        {
            serviceCollection.AddTransient(typeof(IEndpointRegistrar), endpointRegistrarType);
        }
    }

    /// <summary>
    ///     Maps the endpoints from the registered <see cref="IEndpointRegistrar"/> instances to the specified <see cref="IEndpointRouteBuilder"/>
    /// </summary>
    /// <param name="endpoints">
    ///     The <see cref="IEndpointRouteBuilder"/>
    /// </param>
    public static void MapEndpointsFromRegistrars(this IEndpointRouteBuilder endpoints,
                                                  RouteGroupBuilder          groupBuilder = null)
    {
        var endpointRegistrars = endpoints.ServiceProvider
                                          .GetServices<IEndpointRegistrar>()
                                          .ToList();

        foreach (var registrar in endpointRegistrars)
        {
            registrar.Map(groupBuilder ?? endpoints);
        }
    }

    /// <summary>
    ///     Detects and registers all the dependency services,
    ///     that are marked by inheriting from <see cref="IDependency"/> interface,
    ///     found in the <see cref="loadedAssemblies"/>
    ///     to the given <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="loadedAssemblies"></param>
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
                    var existingRegistration = serviceCollection.FirstOrDefault(descriptor =>
                                                                                    descriptor.ServiceType ==
                                                                                    @interface &&
                                                                                    descriptor.ImplementationType ==
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