using System.Threading.Tasks;
using DotNetBrightener.Infrastructure.Security.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace DotNetBrightener.Infrastructure.Security.AuthorizationHandlers;

public class MultiplePermissionsAuthorizationHandler : AuthorizationHandler<PermissionsAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext         context,
                                                   PermissionsAuthorizationRequirement requirement)
    {
        // user not authenticated
        if (context?.User?.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            // stop processing
            return Task.CompletedTask;
        }

        if (context.HasSucceeded)
            // stop processing if already authorized
            return Task.CompletedTask;

        // user must have granted access to ALL given permissions
        foreach (var permissionKey in requirement.PermissionsToAuthorize)
        {
            // if user does not have any of requested permission
            if (!context.User.HasClaim(CommonUserClaimKeys.UserPermission, permissionKey))
            {
                context.Fail();

                return Task.CompletedTask;
            }
        }

        context.Succeed(requirement);

        return Task.CompletedTask;
    }
}