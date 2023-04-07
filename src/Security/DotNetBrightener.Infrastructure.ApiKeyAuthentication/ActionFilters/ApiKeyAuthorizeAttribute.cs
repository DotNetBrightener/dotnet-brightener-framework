using DotNetBrightener.Infrastructure.ApiKeyAuthentication.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetBrightener.Infrastructure.ApiKeyAuthentication.ActionFilters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class ApiKeyAuthorizeAttribute: Attribute, IAsyncAuthorizationFilter
{
    public ApiKeyAuthorizeAttribute()
    {

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

        var isApiKeyAuthorized =
            context.HttpContext.User.Identities.Any(_ => _.IsAuthenticated &&
                                                         _.AuthenticationType ==
                                                         ApiKeyAuthenticationOptions.AuthenticationScheme);

        if (!isApiKeyAuthorized)
        {
            context.Result = new ObjectResult(new
            {
                ErrorMessage = $"Unauthorized access",
                FullErrorMessage = $"Unauthorized access to restricted resource.",

            })
            {
                StatusCode = (int)HttpStatusCode.Unauthorized
            };
            return;
        }

        var apiKeySchemeIdentity =
            context.HttpContext.User.Identities.First(_ => _.AuthenticationType ==
                                                           ApiKeyAuthenticationOptions.AuthenticationScheme);
        var expiryValue = apiKeySchemeIdentity.FindFirst("exp")?.Value;

        if (expiryValue != null &&
            long.TryParse(expiryValue, out var unixExpiryInMinutes))
        {
            var expiredDate = DateTimeOffset.FromUnixTimeSeconds(unixExpiryInMinutes);

            if (expiredDate < DateTimeOffset.Now)
            {
                context.Result = new ObjectResult(new
                {
                    ErrorMessage     = $"API Key Expired",
                    FullErrorMessage = $"Unauthorized access to restricted resource. API Key Expired.",

                })
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };

                return;
            }
        }
    }
}