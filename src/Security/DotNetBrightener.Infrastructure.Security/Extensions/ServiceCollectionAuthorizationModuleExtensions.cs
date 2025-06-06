﻿using DotNetBrightener.Infrastructure.Security.AuthorizationHandlers;
using DotNetBrightener.Infrastructure.Security.HostedService;
using DotNetBrightener.Infrastructure.Security.Providers;
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
        servicesCollection.AddScoped<IAuthorizationHandler, AdministratorAuthorizer>();
        servicesCollection.AddScoped<IAuthorizationHandler, MultiplePermissionsAuthorizationHandler>();

        // permissions and securities
        servicesCollection.AddSingleton<IPermissionsContainer, PermissionsContainer>();

        servicesCollection.RegisterPermissionProvider<DefaultPermissions>();

        servicesCollection.AddHostedService<PermissionLoaderAndValidator>();

        return servicesCollection;
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