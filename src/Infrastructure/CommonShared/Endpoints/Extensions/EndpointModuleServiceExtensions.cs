using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WebApp.CommonShared.Endpoints;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Extension methods for registering endpoint modules and validators.
/// </summary>
public static class EndpointModuleServiceExtensions
{
    /// <summary>
    ///     Adds endpoint modules and optionally validators from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Action to configure endpoint module options</param>
    /// <returns>The service collection for chaining</returns>
    /// <example>
    /// services.AddEndpointModules(options =>
    /// {
    ///     options.Assemblies = AppDomain.CurrentDomain.GetAppOnlyAssemblies();
    ///     options.AutoRegisterValidators = true;
    /// });
    /// </example>
    public static IServiceCollection AddEndpointModules(
        this IServiceCollection services,
        Action<EndpointModuleOptions> configure)
    {
        var options = new EndpointModuleOptions();
        configure(options);

        // Register endpoint modules
        foreach (var assembly in options.Assemblies)
        {
            RegisterEndpointModules(services, assembly);
        }

        // Register validators if enabled
        if (options.AutoRegisterValidators)
        {
            foreach (var assembly in options.Assemblies)
            {
                RegisterValidators(services, assembly);
            }
        }

        return services;
    }

    /// <summary>
    ///     Adds endpoint modules from the specified assemblies without validator registration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">The assemblies to scan for endpoint modules</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEndpointModules(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        return services.AddEndpointModules(options => options.Assemblies = assemblies);
    }

    /// <summary>
    ///     Registers all IEndpointRegistrar implementations from the assembly.
    /// </summary>
    private static void RegisterEndpointModules(IServiceCollection services, Assembly assembly)
    {
        var endpointRegistrarTypes = assembly.GetTypes()
            .Where(t => typeof(IEndpointRegistrar).IsAssignableFrom(t) &&
                        t is { IsClass: true } &&
                        !t.IsAbstract);

        foreach (var type in endpointRegistrarTypes)
        {
            services.AddTransient(typeof(IEndpointRegistrar), type);
        }
    }

    /// <summary>
    ///     Registers all IValidator implementations from the assembly.
    /// </summary>
    private static void RegisterValidators(IServiceCollection services, Assembly assembly)
    {
        var validatorTypes = assembly.GetTypes()
            .Where(t => typeof(IValidator).IsAssignableFrom(t) &&
                        t is { IsClass: true } &&
                        !t.IsAbstract &&
                        !t.IsGenericTypeDefinition);

        foreach (var type in validatorTypes)
        {
            // Find the IValidator<T> interface
            var validatorInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType &&
                                     i.GetGenericTypeDefinition() == typeof(IValidator<>));

            if (validatorInterface != null)
            {
                services.AddTransient(validatorInterface, type);
            }
        }
    }
}
