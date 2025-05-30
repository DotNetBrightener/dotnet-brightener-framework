using DotNetBrightener.MultiTenancy.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace DotNetBrightener.MultiTenancy;

public class TenantDetectionAndCorsEnableMiddleware(
    RequestDelegate next,
    ICorsService    corsService)
{
    public async Task Invoke(HttpContext              context,
                             IHttpContextAccessor     httpContextAccessor,
                             Lazy<ITenantDataService> tenantDataService)
    {
        context.Request
               .Headers
               .TryGetValue(CorsConstants.Origin, out var origin);

        if (string.IsNullOrEmpty(origin))
        {
            await next.Invoke(context);

            return;
        }

        var appHostName = new Uri(origin).GetDomain();
        var tenant      = tenantDataService.Value.GetTenantByHostName(appHostName);

        if (tenant == null)
        {
            await next.Invoke(context);

            return;
        }

        httpContextAccessor.StoreValue(tenant);

        var policyBuilder = new CorsPolicyBuilder();
        policyBuilder.AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();

        policyBuilder.WithOrigins(origin);

        var corsPolicy = policyBuilder.Build();

        var corsResult = corsService.EvaluatePolicy(context, corsPolicy);
        corsService.ApplyResult(corsResult, context.Response);

        var accessControlRequestMethod =
            context.Request.Headers[CorsConstants.AccessControlRequestMethod];

        if (string.Equals(context.Request.Method,
                          CorsConstants.PreflightHttpMethod,
                          StringComparison.Ordinal) &&
            !StringValues.IsNullOrEmpty(accessControlRequestMethod))
        {
            // Since there is a policy which was identified,
            // always respond to preflight requests.
            context.Response.StatusCode = StatusCodes.Status200OK;

            return;
        }

        await next.Invoke(context);
    }
}