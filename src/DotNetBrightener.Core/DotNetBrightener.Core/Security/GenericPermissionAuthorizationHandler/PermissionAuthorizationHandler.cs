using System.Threading.Tasks;
using DotNetBrightener.Core.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace DotNetBrightener.Core.Security.GenericPermissionAuthorizationHandler
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       PermissionRequirement       requirement)
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