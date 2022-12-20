using DotNetBrightener.Infrastructure.Security.Requirements;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DotNetBrightener.Infrastructure.Security.AuthorizationHandlers;

public class AdministratorAuthorizer : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var userRole = context?.User?.FindFirstValue(CommonUserClaimKeys.UserRole);
        if (userRole == null)
        {
            return Task.CompletedTask;
        }

        // if user is an administrator, allow all permissions
        if (string.Equals(userRole, DefaultUserRoles.AdministratorRoleName, StringComparison.OrdinalIgnoreCase))
        {
            GrantAllPermissions(context);
        }

        return Task.CompletedTask;
    }

    private static void GrantAllPermissions(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.Requirements.OfType<PermissionsAuthorizationRequirement>())
        {
            context.Succeed(requirement);
        }
    }
}