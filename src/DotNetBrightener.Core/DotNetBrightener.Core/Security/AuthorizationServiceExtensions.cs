using System.Security.Claims;
using System.Threading.Tasks;
using DotNetBrightener.Core.Permissions;
using DotNetBrightener.Core.Security.GenericPermissionAuthorizationHandler;
using Microsoft.AspNetCore.Authorization;

namespace DotNetBrightener.Core.Security
{
    public static class AuthorizationServiceExtensions
    {
        public static async Task<bool> AuthorizePermissionAsync(this IAuthorizationService service,
                                                                ClaimsPrincipal            user, Permission permission,
                                                                object                     resource = null)
        {
            return (await service.AuthorizeAsync(user, resource, new PermissionRequirement(permission))).Succeeded;
        }

        public static async Task<bool> AuthorizePermissionAsync(this IAuthorizationService service,
                                                                ClaimsPrincipal            user, string permissionKey,
                                                                object                     resource = null)
        {
            return (await service.AuthorizeAsync(user, resource, new PermissionRequirement(permissionKey))).Succeeded;
        }

        public static bool AuthorizePermission(this IAuthorizationService service,    ClaimsPrincipal user,
                                               Permission                 permission, object          resource = null)
        {
            return service.AuthorizeAsync(user, resource, new PermissionRequirement(permission)).Result.Succeeded;
        }

        public static bool AuthorizePermission(this IAuthorizationService service, ClaimsPrincipal user,
                                               string                     permissionKey,
                                               object                     resource = null)
        {
            return service.AuthorizeAsync(user, resource, new PermissionRequirement(permissionKey)).Result.Succeeded;
        }
    }
}