using System.Threading.Tasks;
using DotNetBrightener.Infrastructure.Security.Permissions;
using DotNetBrightener.Infrastructure.Security.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace DotNetBrightener.Infrastructure.Security.AuthorizationHandlers
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       PermissionAuthorizationRequirement requirement)
        {
            // user not authenticated
            if (context?.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                // stop processing
                return Task.CompletedTask;
            }

            // if user already has the requested permission
            if (context.User.HasClaim(Permission.ClaimType, requirement.Permission.PermissionKey))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}