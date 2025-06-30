using DotNetBrightener.MultiTenancy.Entities;
using DotNetBrightener.MultiTenancy.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace DotNetBrightener.MultiTenancy;

public class TenantDetectionMiddleware<TTenant>(
    RequestDelegate next,
    ICorsService    corsService) where TTenant : TenantBase, new()
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task Invoke(HttpContext                       context,
                             IHttpContextAccessor              httpContextAccessor,
                             Lazy<ITenantDataService<TTenant>> tenantDataService)
    {
        TenantBase? detectedTenantByDomain = null;
        TenantBase? detectedTenantById     = null;

        // check current request domain
        var requestDisplayUrl = context.Request.GetRequestUrl();
        var u                 = new Uri(requestDisplayUrl);
        var domain            = (u.Port == 80 || u.Port == 443) ? u.Host : $"{u.Host}:{u.Port}";

        detectedTenantByDomain = await tenantDataService.Value.GetAsync(x => x.TenantDomains != null &&
                                                                              x.TenantDomains.Contains(domain + ";"));

        context.Request
               .Headers
               .TryGetValue(CorsConstants.Origin, out var origin);

        Guid detectedTenantId = Guid.Empty;

        // get current tenant from user claim
        if (context.User?.Identity is { IsAuthenticated: true })
        {
            var tenantIdClaimValues = context.User
                                             .FindFirst(MultiTenantHeaders.CurrentTenantId)
                                            ?.Value;


            if (tenantIdClaimValues != null &&
                Guid.TryParse(tenantIdClaimValues, out detectedTenantId))
            {
                detectedTenantById = await tenantDataService.Value.GetAsync(x => x.Id == detectedTenantId);
            }
        }
        else if (context.Request.Headers.TryGetValue(MultiTenantHeaders.CurrentTenantId,
                                                     out var currentTenantIdHeaderValue) &&
                 Guid.TryParse(currentTenantIdHeaderValue, out var detectedTenantIdViaHeader))
        {
            detectedTenantId = detectedTenantIdViaHeader;
            detectedTenantById = await tenantDataService.Value.GetAsync(x => x.Id == detectedTenantId);
        }

        // if not the same tenant is being requested
        // e.g. user has access from one tenant but making request to the other
        // reject the request
        if (detectedTenantByDomain is not null &&
            detectedTenantById is not null &&
            detectedTenantById.Id != detectedTenantByDomain.Id)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            return;
        }

        if (detectedTenantById is not null)
        {
            httpContextAccessor.StoreValue(MultiTenantCacheKeys.CurrentTenantId, detectedTenantId);
        }

        // If user has access to multiple tenants, see if the current request limits data
        if (context.Request.Headers.TryGetValue(MultiTenantHeaders.LimitTenantIds,
                                                out var limitTenantHeadersValue))
        {
            var limitToTenantIds = limitTenantHeadersValue.ToString()
                                                          .Split([
                                                                     ",", ";"
                                                                 ],
                                                                 StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(s => Guid.TryParse(s, out var tenantId)
                                                                           ? tenantId
                                                                           : Guid.Empty)
                                                          .Where(l => l != Guid.Empty)
                                                          .ToArray();

            httpContextAccessor.StoreValue(MultiTenantCacheKeys.LimitRecordToTenantIds, limitToTenantIds);
        }

        await SetUpCorsPolicyAndResponse(detectedTenantById!, origin, context, _next);
    }

    private async Task SetUpCorsPolicyAndResponse(TenantBase      detectedTenant,
                                                  string          requestOrigin,
                                                  HttpContext     context,
                                                  RequestDelegate next)
    {
        if (string.IsNullOrEmpty(requestOrigin))
        {
            // no origin found, probably request made from API client
            await next.Invoke(context);

            return;
        }

        var isWhitelisted = detectedTenant.WhitelistedOrigins.Contains("*") ||
                            detectedTenant.WhitelistedOrigins.Contains(requestOrigin);

        if (!isWhitelisted)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Origin not allowed.");
            return;
        }

        var policyBuilder = new CorsPolicyBuilder();
        policyBuilder.AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials()
                     .WithOrigins(requestOrigin);

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