using DotNetBrightener.MultiTenancy.Entities;
using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.MultiTenancy;

public class TenantDetectionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task Invoke(HttpContext          context,
                             IHttpContextAccessor httpContextAccessor)
    {
        long[] tenantIdClaimValues = [];

        if (context.User?.Identity != null &&
            context.User.Identity.IsAuthenticated)
        {
            tenantIdClaimValues = context.User.FindAll(MultiTenantConstants.TenantIdentifierClaimKey)
                                         .Select(c => c.Value)
                                         .Select(s =>
                                          {
                                              if (long.TryParse(s, out var tenantId))
                                                  return tenantId;

                                              return -10;
                                          })
                                         .Where(l => l != -10)
                                         .ToArray();
        }


        if (context.Request.Headers.TryGetValue(MultiTenantConstants.LimitTenantIdsHeaderKey,
                                                out var limitTenantHeadersValue))
        {
            var limitToTenantIds = limitTenantHeadersValue.ToString()
                                                          .Split([
                                                                     ",", ";"
                                                                 ],
                                                                 StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(s =>
                                                           {
                                                               if (long.TryParse(s, out var tenantId))
                                                                   return tenantId;

                                                               return -10;
                                                           })
                                                          .Where(l => l != -10)
                                                          .ToArray();

            httpContextAccessor.StoreValue(MultiTenantConstants.LimitRecordToTenantIds, limitToTenantIds);

            if (limitToTenantIds.Length > 0)
            {
                tenantIdClaimValues = tenantIdClaimValues.Concat(limitToTenantIds).Distinct().ToArray();
            }
        }

        var detectedTenant = httpContextAccessor.RetrieveValue<Tenant>();

        if (detectedTenant != null)
        {
            tenantIdClaimValues = tenantIdClaimValues.Concat([
                                                          detectedTenant.Id
                                                      ])
                                                     .Distinct()
                                                     .ToArray();
        }

        if (tenantIdClaimValues.Length > 0)
            httpContextAccessor.StoreValue(MultiTenantConstants.TenantIdsContextKey, tenantIdClaimValues);

        await _next(context);
    }
}