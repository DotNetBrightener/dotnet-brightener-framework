using System.Security.Claims;
using DotNetBrightener.Infrastructure.Security.MinimalApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
///     Extension methods for permission-based authorization on Minimal API endpoints
/// </summary>
public static class RouteHandlerBuilderExtensions
{
    /// <summary>
    ///     Requires the specified permission(s) for the endpoint.
    ///     User must have ALL specified permissions to access the endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder</param>
    /// <param name="permissions">Permission keys required to access the endpoint</param>
    /// <returns>The route handler builder for chaining</returns>
    /// <example>
    /// <code>
    /// app.MapGet("/api/users", GetUsers)
    ///    .RequirePermission("UserManagement.View");
    /// </code>
    /// </example>
    public static RouteHandlerBuilder RequirePermission(
        this RouteHandlerBuilder builder,
        params string[] permissions)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var authService = context.HttpContext.RequestServices
                .GetRequiredService<IAuthorizationService>();

            var filter = new PermissionAuthorizeFilter(authService, permissions);
            return await filter.InvokeAsync(context, next);
        });

        return builder;
    }

    /// <summary>
    ///     Requires the specified permission for the endpoint with a resource for resource-based authorization.
    ///     User must have the permission to access the specified resource.
    /// </summary>
    /// <param name="builder">The route handler builder</param>
    /// <param name="permission">Permission key required to access the endpoint</param>
    /// <param name="resource">The resource to authorize access against</param>
    /// <returns>The route handler builder for chaining</returns>
    /// <example>
    /// <code>
    /// app.MapGet("/api/documents/{id}", GetDocument)
    ///    .RequirePermission("Document.View", document);
    /// </code>
    /// </example>
    public static RouteHandlerBuilder RequirePermission(
        this RouteHandlerBuilder builder,
        string permission,
        object resource)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var authService = context.HttpContext.RequestServices
                .GetRequiredService<IAuthorizationService>();

            var filter = new PermissionAuthorizeFilter(authService, new[] { permission }, resource);
            return await filter.InvokeAsync(context, next);
        });

        return builder;
    }

    /// <summary>
    ///     Requires the specified permission(s) for the endpoint with a resource for resource-based authorization.
    ///     User must have ALL specified permissions to access the specified resource.
    /// </summary>
    /// <param name="builder">The route handler builder</param>
    /// <param name="resource">The resource to authorize access against</param>
    /// <param name="permissions">Permission keys required to access the endpoint</param>
    /// <returns>The route handler builder for chaining</returns>
    public static RouteHandlerBuilder RequirePermissionForResource(
        this RouteHandlerBuilder builder,
        object resource,
        params string[] permissions)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var authService = context.HttpContext.RequestServices
                .GetRequiredService<IAuthorizationService>();

            var filter = new PermissionAuthorizeFilter(authService, permissions, resource);
            return await filter.InvokeAsync(context, next);
        });

        return builder;
    }

    /// <summary>
    ///     Requires ANY of the specified permissions for the endpoint.
    ///     User must have at least ONE of the specified permissions to access the endpoint.
    /// </summary>
    /// <param name="builder">The route handler builder</param>
    /// <param name="permissions">Permission keys (any one is sufficient to access the endpoint)</param>
    /// <returns>The route handler builder for chaining</returns>
    /// <example>
    /// <code>
    /// app.MapGet("/api/reports", GetReports)
    ///    .RequireAnyPermission("Reports.View", "Reports.ViewAll", "Admin.Access");
    /// </code>
    /// </example>
    public static RouteHandlerBuilder RequireAnyPermission(
        this RouteHandlerBuilder builder,
        params string[] permissions)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var authService = context.HttpContext.RequestServices
                .GetRequiredService<IAuthorizationService>();

            var filter = new PermissionAuthorizeFilter(authService, permissions, requireAll: false);
            return await filter.InvokeAsync(context, next);
        });

        return builder;
    }

    /// <summary>
    ///     Requires ANY of the specified permissions for the endpoint with a resource for resource-based authorization.
    ///     User must have at least ONE of the specified permissions to access the specified resource.
    /// </summary>
    /// <param name="builder">The route handler builder</param>
    /// <param name="resource">The resource to authorize access against</param>
    /// <param name="permissions">Permission keys (any one is sufficient to access the endpoint)</param>
    /// <returns>The route handler builder for chaining</returns>
    public static RouteHandlerBuilder RequireAnyPermissionForResource(
        this RouteHandlerBuilder builder,
        object resource,
        params string[] permissions)
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var authService = context.HttpContext.RequestServices
                .GetRequiredService<IAuthorizationService>();

            var filter = new PermissionAuthorizeFilter(authService, permissions, resource, requireAll: false);
            return await filter.InvokeAsync(context, next);
        });

        return builder;
    }
}
