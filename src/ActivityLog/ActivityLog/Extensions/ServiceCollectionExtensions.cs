using ActivityLog;
using ActivityLog.ActionFilters;
using ActivityLog.Configuration;
using ActivityLog.Interceptors;
using ActivityLog.Services;
using Castle.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using VampireCoder.SharedUtils.DependencyInjection;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds activity logging services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Optional configuration section for activity logging</param>
    /// <param name="configureOptions">Optional action to configure activity logging options</param>
    /// <param name="assembliesToScan">Optional assemblies to scan for services to intercept</param>
    /// <returns>The ActivityLogBuilder for chaining</returns>
    public static ActivityLogBuilder AddActivityLogging(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<ActivityLogConfiguration>? configureOptions = null,
        Assembly[]? assembliesToScan = null)
    {
        // Configure options
        if (configuration != null)
        {
            services.Configure<ActivityLogConfiguration>(configuration);
        }

        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        // Register core services
        services.TryAddSingleton<IProxyGenerator, ProxyGenerator>();
        services.TryAddScoped<ActivityLogInterceptor>();
        services.TryAddScoped<IActivityLogContextProvider, ActivityLogContextProvider>();
        services.TryAddSingleton<IActivityLogContextAccessor, ActivityLogContextAccessor>();

        services.TryAddSingleton<IActivityLogSerializer, ActivityLogSerializer>();
        services.TryAddSingleton<IActivityLogService, ActivityLogService>();

        // Register and immediately initialize the static accessor
        services.AddSingleton<ActivityLogContextAccessorInitializer>(provider =>
        {
            var accessor = provider.GetRequiredService<IActivityLogContextAccessor>();
            return new ActivityLogContextAccessorInitializer(accessor);
        });

        // Create and register builder
        var builder = new ActivityLogBuilder
        {
            Services = services
        };
        services.AddSingleton(builder);

        // Scan and register intercepted services
        assembliesToScan ??= AppDomain.CurrentDomain.GetAssemblies();
        RegisterInterceptedServices(services, assembliesToScan);

        return builder;
    }

    private static void RegisterInterceptedServices(IServiceCollection services, Assembly[] assembliesToScan)
    {
        var servicesToIntercept = new List<ServiceDescriptor>();

        // Find services that should be intercepted
        foreach (var assembly in assembliesToScan)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && HasLogActivityAttribute(t))
                    .ToList();

                foreach (var implementationType in types)
                {
                    var interfaces = implementationType.GetInterfaces()
                        .Where(i => i.IsPublic && !i.IsGenericTypeDefinition)
                        .ToList();

                    foreach (var serviceType in interfaces)
                    {
                        // Check if this service is already registered
                        var existingService = services.FirstOrDefault(s => s.ServiceType == serviceType);
                        if (existingService != null)
                        {
                            servicesToIntercept.Add(existingService);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Handle assemblies that can't be fully loaded
                var loadableTypes = ex.Types.Where(t => t != null).ToArray();
                // Process loadable types if needed
            }
            catch (Exception)
            {
                // Skip assemblies that can't be processed
                continue;
            }
        }

        // Replace services with intercepted versions
        foreach (var serviceDescriptor in servicesToIntercept)
        {
            if (serviceDescriptor.ServiceType.IsInterface && 
                serviceDescriptor.ImplementationType != null)
            {
                // Remove original registration
                services.Remove(serviceDescriptor);

                // Add intercepted version
                var method = typeof(ServiceCollectionExtensions)
                    .GetMethod(nameof(AddInterceptedServiceInternal), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(serviceDescriptor.ServiceType, serviceDescriptor.ImplementationType);

                method.Invoke(null, new object[] { services, serviceDescriptor.Lifetime });
            }
        }
    }

    private static void AddInterceptedServiceInternal<TService, TImplementation>(
        IServiceCollection services, 
        ServiceLifetime lifetime)
        where TService : class
        where TImplementation : class, TService
    {
        switch (lifetime)
        {
            case ServiceLifetime.Scoped:
                services.AddInterceptedScoped<TService, TImplementation, ActivityLogInterceptor>();
                break;
            case ServiceLifetime.Singleton:
                services.AddInterceptedSingleton<TService, TImplementation, ActivityLogInterceptor>();
                break;
            case ServiceLifetime.Transient:
                services.AddInterceptedTransient<TService, TImplementation, ActivityLogInterceptor>();
                break;
        }
    }

    private static bool HasLogActivityAttribute(Type type)
    {
        // Only check if any methods have the LogActivity attribute (not class-level)
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Any(m => m.GetCustomAttribute<LogActivityAttribute>() != null);
    }
}

/// <summary>
/// Initializes the static ActivityLogContext accessor
/// </summary>
internal class ActivityLogContextAccessorInitializer
{
    public ActivityLogContextAccessorInitializer(IActivityLogContextAccessor accessor)
    {
        ActivityLogContext.SetAccessor(accessor);
    }
}