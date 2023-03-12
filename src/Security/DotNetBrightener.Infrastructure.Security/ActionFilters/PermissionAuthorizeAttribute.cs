using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        var authorizationService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();

        if (!await authorizationService.AuthorizePermissionAsync(context.HttpContext.User,
                                                                  string.Join(",", permissionsToValidate)))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                ErrorMessage = $"Unauthorized access to restricted resource",
                FullErrorMessage =
                    $"Unauthorized access to restricted resource. Required permissions {string.Join(", ", permissionsToValidate)}",
            });
        }

        context.HttpContext.StoreValue(PermissionProcessed, true);
    }
}