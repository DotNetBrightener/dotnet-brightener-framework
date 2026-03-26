using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WebApp.CommonShared.Endpoints;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

/// <summary>
///     Extension methods for mapping endpoint modules to the route builder.
/// </summary>
public static class EndpointModuleRouteExtensions
{
    /// <summary>
    ///     Maps all registered endpoint modules to the route builder.
    ///     This method retrieves all IEndpointRegistrar instances and invokes their Map method.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder</param>
    /// <returns>The endpoint route builder for chaining</returns>
    /// <example>
    /// app.MapEndpointModules();
    /// </example>
    public static IEndpointRouteBuilder MapEndpointModules(this IEndpointRouteBuilder endpoints)
    {
        var endpointRegistrars = endpoints.ServiceProvider
            .GetServices<IEndpointRegistrar>()
            .ToList();

        foreach (var registrar in endpointRegistrars)
        {
            registrar.Map(endpoints);
        }

        return endpoints;
    }

    /// <summary>
    ///     Maps all registered endpoint modules to a route group.
    ///     Use this when you want to apply common configuration to all endpoints.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder</param>
    /// <param name="basePath">The base path for all endpoints</param>
    /// <param name="configureGroup">Optional action to configure the route group</param>
    /// <returns>The route group builder for further configuration</returns>
    /// <example>
    /// app.MapEndpointModules("/api/v1", group =>
    /// {
    ///     group.RequireAuthorization();
    ///     group.WithOpenApi();
    /// });
    /// </example>
    public static RouteGroupBuilder MapEndpointModules(
        this IEndpointRouteBuilder endpoints,
        string basePath,
        Action<RouteGroupBuilder>? configureGroup = null)
    {
        var group = endpoints.MapGroup(basePath);

        configureGroup?.Invoke(group);

        var endpointRegistrars = endpoints.ServiceProvider
            .GetServices<IEndpointRegistrar>()
            .ToList();

        foreach (var registrar in endpointRegistrars)
        {
            registrar.Map(group);
        }

        return group;
    }
}
