using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace WebApp.CommonShared.Endpoints;

/// <summary>
///     Base class for endpoint modules with route grouping and cross-cutting concerns support.
///     Provides a structured way to organize related endpoints with shared configuration.
/// </summary>
public abstract class EndpointModuleBase : IEndpointRegistrar
{
    /// <summary>
    ///     Gets the base path for all routes in this module.
    ///     Example: "/api/users" will group all routes under this path.
    /// </summary>
    protected virtual string BasePath => string.Empty;

    /// <summary>
    ///     Gets the tags for OpenAPI documentation.
    ///     Used to group related endpoints in Swagger UI.
    /// </summary>
    protected virtual string[] Tags => Array.Empty<string>();

    /// <summary>
    ///     Configures the route group with cross-cutting concerns.
    ///     Apply authorization, CORS, rate limiting, caching, etc. here.
    /// </summary>
    /// <example>
    /// protected override Action&lt;RouteGroupBuilder&gt;? ConfigureGroup => group =>
    /// {
    ///     group.RequireAuthorization();
    ///     group.WithTags("Users");
    ///     group.RequireCors("DefaultPolicy");
    /// };
    /// </example>
    protected virtual Action<RouteGroupBuilder>? ConfigureGroup => null;

    /// <summary>
    ///     Maps the endpoints to the route builder.
    ///     This method handles route group creation and configuration.
    /// </summary>
    /// <param name="app">The endpoint route builder to map endpoints to</param>
    public void Map(IEndpointRouteBuilder app)
    {
        var group = CreateRouteGroup(app);
        ApplyGroupConfiguration(group);
        MapEndpoints(group);
    }

    /// <summary>
    ///     Creates the route group based on BasePath.
    ///     Override to customize group creation behavior.
    /// </summary>
    protected virtual RouteGroupBuilder CreateRouteGroup(IEndpointRouteBuilder app)
    {
        return string.IsNullOrEmpty(BasePath)
            ? app.MapGroup(string.Empty)
            : app.MapGroup(BasePath);
    }

    /// <summary>
    ///     Applies configuration to the route group.
    ///     Handles Tags and ConfigureGroup application.
    /// </summary>
    protected virtual void ApplyGroupConfiguration(RouteGroupBuilder group)
    {
        if (Tags.Length > 0)
        {
            group.WithTags(Tags);
        }

        ConfigureGroup?.Invoke(group);
    }

    /// <summary>
    ///     Override to define your endpoints.
    ///     Use the provided builder to map GET, POST, PUT, DELETE, etc. routes.
    /// </summary>
    /// <param name="app">The endpoint route builder (either the group or root builder)</param>
    /// <example>
    /// protected override void MapEndpoints(IEndpointRouteBuilder app)
    /// {
    ///     app.MapGet("/", GetAllUsers);
    ///     app.MapPost("/", CreateUser);
    ///     app.MapGet("/{id}", GetUserById);
    /// }
    /// </example>
    protected abstract void MapEndpoints(IEndpointRouteBuilder app);
}
