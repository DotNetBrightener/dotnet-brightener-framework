using DotNetBrightener.MultiTenancy.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetBrightener.MultiTenancy;

public class TenantDetectionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantDetectionMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task Invoke(HttpContext          context,
                             IHttpContextAccessor httpContextAccessor)
    {
        long[] tenantIdClaimValues = Array.Empty<long>();

        if (context.User?.Identity != null &&
            context.User.Identity.IsAuthenticated)
        {
            tenantIdClaimValues = context.User.FindAll(MultiTenantConstants.TenantIdentifierClaimKey)
                                         .Select(_ => _.Value)
                                         .Select(_ =>
                                          {
                                              if (long.TryParse(_, out var tenantId))
                                                  return tenantId;

                                              return -10;
                                          })
                                         .Where(_ => _ != -10)
                                         .ToArray();
        }


        if (context.Request.Headers.TryGetValue(MultiTenantConstants.LimitTenantIdsHeaderKey,
                                                out var limitTenantHeadersValue))
        {
            var limitToTenantIds = limitTenantHeadersValue.ToString()
                                                          .Split(new[]
                                                                 {
                                                                     ",", ";"
                                                                 },
                                                                 StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(_ =>
                                                           {
                                                               if (long.TryParse(_, out var tenantId))
                                                                   return tenantId;

                                                               return -10;
                                                           })
                                                          .Where(_ => _ != -10)
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
            tenantIdClaimValues = tenantIdClaimValues.Concat(new[]
                                                      {
                                                          detectedTenant.Id
                                                      })
                                                     .Distinct()
                                                     .ToArray();
        }

        if (tenantIdClaimValues.Length > 0)
            httpContextAccessor.StoreValue(MultiTenantConstants.TenantIdsContextKey, tenantIdClaimValues);

        await _next(context);
    }
}