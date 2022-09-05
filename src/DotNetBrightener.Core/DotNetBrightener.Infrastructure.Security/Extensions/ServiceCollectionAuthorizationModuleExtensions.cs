using DotNetBrightener.Infrastructure.Security.AuthorizationHandlers;
using DotNetBrightener.Infrastructure.Security.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using System.Linq;

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
        servicesCollection.AddScoped<IAuthorizationHandler, AdministratorAuthorizer>();
        servicesCollection.AddScoped<IAuthorizationHandler, MultiplePermissionsAuthorizationHandler>();

        // permissions and securities
        servicesCollection.AddSingleton<IPermissionsContainer, PermissionsContainer>();

        return servicesCollection;
    }

    /// <summary>
    ///     Loads permissions into the application pipeline and validates to prevent duplications
    /// </summary>
    /// <param name="appBuilder">
    ///     The application pipeline
    /// </param>
    public static void LoadAndValidatePermissions(this IApplicationBuilder appBuilder)
    {
        using (var scope = appBuilder.ApplicationServices.CreateScope())
        {
            var permissionContainer = scope.ServiceProvider.GetService<IPermissionsContainer>();

            permissionContainer?.LoadAndValidatePermissions();
        }
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
        var existingTypeRegistras = servicesCollection.Where(_ => _.ImplementationType == typeof(TPermissionProvider))
                                                      .ToList();

        foreach (var registra in existingTypeRegistras)
        {
            servicesCollection.Remove(registra);
        }

        servicesCollection.AddSingleton<IPermissionProvider, TPermissionProvider>();

        return servicesCollection;
    }
}