using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.Core.Authentication;

public class OverrideAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
        
    public OverrideAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task Invoke(HttpContext                   context,
                             IAuthenticationSchemeProvider schemes)
    {
        var hasCookie              = context.Request.Cookies.Count > 0;
        var hasAuthorizationHeader = context.Request.Headers.ContainsKey("Authorization");

        if (!hasCookie && !hasAuthorizationHeader)
        {
            await _next(context);
            return;
        }

        // default to cookie.
        var schemeNameForValidate = CookieAuthenticationDefaults.AuthenticationScheme;

        // detect if there is Authorization header, with the format of JWT. Throw exception if not
        if (hasAuthorizationHeader &&
            context.Request.Headers.TryGetValue("Authorization", out var authorizationHeaderValue))
        {
            if (authorizationHeaderValue.ToString().StartsWith(JwtBearerDefaults.AuthenticationScheme))
            {
                schemeNameForValidate = JwtBearerDefaults.AuthenticationScheme;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported AuthenticationScheme detected");
            }
        }

        var authenticateSchemeAsync = await schemes.GetSchemeAsync(schemeNameForValidate);

        if (authenticateSchemeAsync != null)
        {
            var authenticateResult = await context.AuthenticateAsync(authenticateSchemeAsync.Name);
            if (authenticateResult?.Principal != null)
            {
                context.User = authenticateResult.Principal;
            }
        }

        await _next(context);
    }
}