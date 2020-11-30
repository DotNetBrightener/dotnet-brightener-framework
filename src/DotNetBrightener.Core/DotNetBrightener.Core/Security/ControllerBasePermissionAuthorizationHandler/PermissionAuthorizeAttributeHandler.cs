using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetBrightener.Core.Permissions;
using DotNetBrightener.Core.Security.ControllerBasePermissionAuthorizationHandler.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Core.Security.ControllerBasePermissionAuthorizationHandler
{
    public class PermissionAuthorizeAttributeHandler : AuthorizationHandler<PermissionAuthorizeAttributeRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionAuthorizeAttributeHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext             context,
                                                       PermissionAuthorizeAttributeRequirement requirement)
        {
            using var scope = _httpContextAccessor.HttpContext.RequestServices.CreateScope();

            var actionContext = context.Resource as FilterContext;

            var grantedPermissions = new List<string>();

            if (actionContext?.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                var actionAttributes =
                    controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true,
                                                                              attributeType:
                                                                              typeof(PermissionAuthorizeAttribute));

                var permissionAttributes = actionAttributes.OfType<PermissionAuthorizeAttribute>()
                                                           .SelectMany(x => x.GetPermissions());

                grantedPermissions.AddRange(permissionAttributes);
            }

            // if no permissions specified, then user should have access as the resource is not restricting at all.
            if (!grantedPermissions.Any())
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // in case permissions are specified, check whether the JWT token contains the permission claims.
            if (!context.User.HasClaim(c => c.Type == Permission.ClaimType))
            {
                // if no permission claims, then restrict resource
                return Task.CompletedTask;
            }

            // get all permissions from JWT claims
            var userPermissions = context.User
                                         .FindAll(claim => claim.Type == Permission.ClaimType)
                                         .Select(claim => claim.Value)
                                         .ToArray();

            // if user's permissions have any that is granted by the resource, then allow accessing.
            if (userPermissions.Any(permission => grantedPermissions.Contains(permission)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}