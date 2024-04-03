using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Infrastructure.Security.ActionFilters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class PermissionAuthorizeAttribute: Attribute, IAsyncAuthorizationFilter
{
    private readonly string _permission;
    private const    string PermissionProcessed = "PERMISSIONS_PROCESSED";

    public PermissionAuthorizeAttribute()
    {

    }

    public PermissionAuthorizeAttribute(string permission)
    {
        _permission = permission;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor cad)
            return;

        if (cad.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>() != null ||
            cad.ControllerTypeInfo.GetCustomAttribute<AllowAnonymousAttribute>() != null)
        {
            return;
        }

        if (context.HttpContext.RetrieveValue<bool>(PermissionProcessed))
        {
            return;
        }

        var permissionAuthorizeAttr = cad.MethodInfo
                                         .GetCustomAttributes<PermissionAuthorizeAttribute>()
                                         .ToArray();

        if (!permissionAuthorizeAttr.Any())
            permissionAuthorizeAttr = cad.ControllerTypeInfo
                                         .GetCustomAttributes<PermissionAuthorizeAttribute>()
                                         .ToArray();
        
        var permissionsToValidate = permissionAuthorizeAttr.Select(_ => _._permission)
                                                           .ToArray();

        if (!permissionsToValidate.Any())
        {
            context.HttpContext.StoreValue(PermissionProcessed, true);
            return;
        }

        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var authorizationService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();

        if (!await authorizationService.AuthorizePermissionAsync(context.HttpContext.User,
                                                                  string.Join(",", permissionsToValidate)))
        {
            context.Result = new ObjectResult(new
            {
                ErrorMessage     = $"Unauthorized access to restricted resource",
                FullErrorMessage = $"Unauthorized access to restricted resource.",
                Data = new
                {
                    RequiredPermissions = permissionsToValidate
                }
            })
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };

        }

        context.HttpContext.StoreValue(PermissionProcessed, true);
    }
}