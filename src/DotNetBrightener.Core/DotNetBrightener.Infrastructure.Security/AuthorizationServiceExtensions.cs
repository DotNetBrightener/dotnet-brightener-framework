using System.Security.Claims;
using System.Threading.Tasks;
using DotNetBrightener.Infrastructure.Security.Permissions;
using DotNetBrightener.Infrastructure.Security.Requirements;

// ReSharper disable CheckNamespace
namespace Microsoft.AspNetCore.Authorization;

public static class AuthorizationServiceExtensions
{
    /// <summary>
    ///     Checks if the given <paramref name="user" /> meets the permission requirement for the specified <paramref name="resource" />
    /// </summary>
    /// <param name="service"></param>
    /// <param name="user"></param>
    /// <param name="permission"></param>
    /// <param name="resource"></param>
    /// <returns></returns>
    public static async Task<bool> AuthorizePermissionAsync(this IAuthorizationService service,
                                                            ClaimsPrincipal            user,
                                                            Permission                 permission,
                                                            object                     resource = null)
    {
        return await AuthorizePermissionAsync(service, user, permission.PermissionKey, resource);
    }

    /// <summary>
    ///     Checks if the given <paramref name="user" /> meets the permission requirement for the specified <paramref name="resource" />
    /// </summary>
    /// <param name="service"></param>
    /// <param name="user"></param>
    /// <param name="permissionKey"></param>
    /// <param name="resource"></param>
    /// <returns></returns>
    public static async Task<bool> AuthorizePermissionAsync(this IAuthorizationService service,
                                                            ClaimsPrincipal            user,
                                                            string                     permissionKey,
                                                            object                     resource = null)
    {
        var authorizationRequirements = new PermissionsAuthorizationRequirement(permissionKey);

        var authorizationResult = await service.AuthorizeAsync(user,
                                                               resource,
                                                               authorizationRequirements);

        return authorizationResult.Succeeded;
    }

    /// <summary>
    ///     Checks if the given <paramref name="user" /> meets the permission requirement for the specified <paramref name="resource" />
    /// </summary>
    /// <param name="service"></param>
    /// <param name="user"></param>
    /// <param name="permission"></param>
    /// <param name="resource"></param>
    /// <returns></returns>
    public static bool AuthorizePermission(this IAuthorizationService service,
                                           ClaimsPrincipal            user,
                                           Permission                 permission,
                                           object                     resource = null)
    {
        return AuthorizePermissionAsync(service, user, permission.PermissionKey, resource).Result;
    }

    /// <summary>
    ///     Checks if the given <paramref name="user" /> meets the permission requirement for the specified <paramref name="resource" />
    /// </summary>
    /// <param name="service"></param>
    /// <param name="user"></param>
    /// <param name="permissionKey"></param>
    /// <param name="resource"></param>
    /// <returns></returns>
    public static bool AuthorizePermission(this IAuthorizationService service,
                                           ClaimsPrincipal            user,
                                           string                     permissionKey,
                                           object                     resource = null)
    {
        return AuthorizePermissionAsync(service, user, permissionKey, resource).Result;
    }
}