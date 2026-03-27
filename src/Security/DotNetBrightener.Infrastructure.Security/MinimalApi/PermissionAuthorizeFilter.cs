#nullable enable
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Infrastructure.Security.MinimalApi;

/// <summary>
///     Endpoint filter that authorizes requests based on required permissions.
///     Supports both AND-based (all permissions required) and OR-based (any permission required) logic.
/// </summary>
public class PermissionAuthorizeFilter : IEndpointFilter
{
    private readonly string[] _permissions;
    private readonly object? _resource;
    private readonly IAuthorizationService _authorizationService;
    private readonly bool _requireAll;

    /// <summary>
    ///     Initializes a new instance of <see cref="PermissionAuthorizeFilter" />
    /// </summary>
    /// <param name="authorizationService">The authorization service</param>
    /// <param name="permissions">Permission keys required to access the endpoint</param>
    /// <param name="resource">Optional resource for resource-based authorization</param>
    /// <param name="requireAll">If true, all permissions are required; if false, any one permission is sufficient</param>
    public PermissionAuthorizeFilter(
        IAuthorizationService authorizationService,
        string[] permissions,
        object? resource = null,
        bool requireAll = true)
    {
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _permissions = permissions ?? Array.Empty<string>();
        _resource = resource;
        _requireAll = requireAll;
    }

    /// <summary>
    ///     Invokes the filter to check permission authorization
    /// </summary>
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var endpoint = httpContext.GetEndpoint();

        // Skip if AllowAnonymous is present
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            return await next(context);
        }

        // Check authentication
        if (httpContext.User?.Identity?.IsAuthenticated != true)
        {
            return Results.Unauthorized();
        }

        // No permissions required, allow
        if (_permissions.Length == 0)
        {
            return await next(context);
        }

        // Authorize based on requireAll flag
        bool authorized;

        if (_requireAll)
        {
            authorized = await _authorizationService.AuthorizePermissionAsync(
                httpContext.User,
                string.Join(",", _permissions),
                _resource);
        }
        else
        {
            // OR-based: check each permission, succeed if any passes
            authorized = await CheckAnyPermissionAsync(httpContext.User);
        }

        if (!authorized)
        {
            return Results.Json(new
            {
                ErrorMessage = "Unauthorized access to restricted resource",
                FullErrorMessage = "Unauthorized access to restricted resource.",
                Data = new
                {
                    RequiredPermissions = _permissions,
                    RequirementType = _requireAll ? "All" : "Any"
                }
            }, statusCode: (int)HttpStatusCode.Forbidden);
        }

        return await next(context);
    }

    /// <summary>
    ///     Checks if the user has ANY of the required permissions
    /// </summary>
    private async Task<bool> CheckAnyPermissionAsync(ClaimsPrincipal user)
    {
        foreach (var permission in _permissions)
        {
            var result = await _authorizationService.AuthorizePermissionAsync(
                user,
                permission,
                _resource);

            if (result)
            {
                return true;
            }
        }

        return false;
    }
}
