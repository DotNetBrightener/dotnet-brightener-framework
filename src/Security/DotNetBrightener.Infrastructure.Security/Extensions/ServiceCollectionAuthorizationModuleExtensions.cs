using DotNetBrightener.Infrastructure.Security.AuthorizationHandlers;
using DotNetBrightener.Infrastructure.Security.HostedService;
using DotNetBrightener.Infrastructure.Security.Services;
using Microsoft.AspNetCore.Authorization;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionAuthorizationModuleExtensions
{
    /// <summary>
    ///     Adds required services for permission authorization into the given <paramref name="servicesCollection" />
    /// </summary>
    /// <param name="servicesCollection">The <see cref="IServiceCollection" /></param>
    /// <returns>
    ///     The same instance of <paramref name="servicesCollection" /> for chaining operations
    /// </returns>
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection servicesCollection)
    {
        servicesCollection.AddScoped<IAuthorizationHandler, SysAdminAuthorizer>();
        servicesCollection.AddScoped<IAuthorizationHandler, MultiplePermissionsAuthorizationHandler>();

        // permissions and securities
        servicesCollection.AddSingleton<IPermissionsContainer, PermissionsContainer>();
        
        servicesCollection.AddHostedService<PermissionLoaderAndValidator>();

        return servicesCollection;
    }

    /// <summary>
    ///     Registers the System Admin Provider to the <see cref="IServiceCollection"/>
    /// </summary>
    /// <typeparam name="TSysAdminProvider">
    ///     The implementation of <seealso cref="ISysAdminRoleProvider"/> that provides roles to consider as Sys Admin
    /// </typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /></param>
    /// <returns>
    ///     The same instance of <paramref name="servicesCollection" /> for chaining operations
    /// </returns>
    public static IServiceCollection AddSystemAdminProvider<TSysAdminProvider>(
        this IServiceCollection serviceCollection)
        where TSysAdminProvider : class, ISysAdminRoleProvider
    {
        serviceCollection.AddSingleton<ISysAdminRoleProvider, TSysAdminProvider>();

        return serviceCollection;
    }

    /// <summary>
    ///     Register a permission provider implementation of type <typeparamref name="TPermissionProvider"/> into the service collection
    /// </summary>
    /// <typeparam name="TPermissionProvider">The type of permission provider that implements <see cref="IPermissionProvider"/></typeparam>        
    /// <param name="servicesCollection">The <see cref="IServiceCollection" /></param>
    /// <returns>
    ///     The same instance of <paramref name="servicesCollection" /> for chaining operations
    /// </returns>
    public static IServiceCollection RegisterPermissionProvider<TPermissionProvider>(
        this IServiceCollection servicesCollection)
        where TPermissionProvider : class, IPermissionProvider
    {
        var existingTypeRegistrations = servicesCollection
                                       .Where(sd => sd.ImplementationType == typeof(TPermissionProvider))
                                       .ToList();

        foreach (var serviceDescriptor in existingTypeRegistrations)
        {
            servicesCollection.Remove(serviceDescriptor);
        }

        servicesCollection.AddSingleton<IPermissionProvider, TPermissionProvider>();

        return servicesCollection;
    }
}