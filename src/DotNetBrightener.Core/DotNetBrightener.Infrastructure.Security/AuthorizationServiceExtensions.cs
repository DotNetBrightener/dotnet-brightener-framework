using System.Security.Claims;
using System.Threading.Tasks;
using DotNetBrightener.Infrastructure.Security.Permissions;
using DotNetBrightener.Infrastructure.Security.Requirements;

namespace Microsoft.AspNetCore.Authorization
{
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
                                                                ClaimsPrincipal user,
                                                                Permission permission,
                                                                object resource = null)
        {
            return (await service.AuthorizeAsync(user, resource, new PermissionAuthorizationRequirement(permission))).Succeeded;
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
                                                                ClaimsPrincipal user,
                                                                string permissionKey,
                                                                object resource = null)
        {
            return (await service.AuthorizeAsync(user, resource, new PermissionAuthorizationRequirement(permissionKey))).Succeeded;
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
                                               ClaimsPrincipal user,
                                               Permission permission,
                                               object resource = null)
        {
            return AuthorizePermissionAsync(service, user, permission, resource).Result;
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
                                               ClaimsPrincipal user,
                                               string permissionKey,
                                               object resource = null)
        {
            return AuthorizePermission(service, user, new Permission(permissionKey), resource);
        }
    }
}