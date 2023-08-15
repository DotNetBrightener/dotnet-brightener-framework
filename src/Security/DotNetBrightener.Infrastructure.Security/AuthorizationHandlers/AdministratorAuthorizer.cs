using DotNetBrightener.Infrastructure.Security.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DotNetBrightener.Infrastructure.Security.AuthorizationHandlers;

public class AdministratorAuthorizer : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.HasRole(DefaultUserRoles.AdministratorRoleName) || 
            context.User.IsInRole(DefaultUserRoles.AdministratorRoleName))
        {
            GrantAllPermissions(context);
        }

        return Task.CompletedTask;
    }

    private static void GrantAllPermissions(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.Requirements
                                           .OfType<PermissionsAuthorizationRequirement>())
        {
            context.Succeed(requirement);
        }
    }
}