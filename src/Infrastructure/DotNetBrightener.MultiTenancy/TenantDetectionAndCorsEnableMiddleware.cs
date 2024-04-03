using DotNetBrightener.MultiTenancy.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace DotNetBrightener.MultiTenancy;

public class TenantDetectionAndCorsEnableMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICorsService    _corsService;

    public TenantDetectionAndCorsEnableMiddleware(RequestDelegate next,
                                                  ICorsService    corsService)
    {
        _next        = next;
        _corsService = corsService;
    }

    public async Task Invoke(HttpContext              context,
                             IHttpContextAccessor     httpContextAccessor,
                             Lazy<ITenantDataService> tenantDataService)
    {
        context.Request
               .Headers
               .TryGetValue(CorsConstants.Origin, out var origin);

        if (string.IsNullOrEmpty(origin))
        {
            await _next.Invoke(context);

            return;
        }

        var appHostName = new Uri(origin).GetDomain();
        var tenant      = tenantDataService.Value.GetTenantByHostName(appHostName);

        if (tenant == null)
        {
            await _next.Invoke(context);

            return;
        }

        httpContextAccessor.StoreValue(tenant);

        var policyBuilder = new CorsPolicyBuilder();
        policyBuilder.AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();

        policyBuilder.WithOrigins(origin);

        var corsPolicy = policyBuilder.Build();

        if (corsPolicy != null)
        {
            var corsResult = _corsService.EvaluatePolicy(context, corsPolicy);
            _corsService.ApplyResult(corsResult, context.Response);

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
        }

        await _next.Invoke(context);
    }
}