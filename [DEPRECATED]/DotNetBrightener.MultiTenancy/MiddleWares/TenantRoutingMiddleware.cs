using System.Threading.Tasks;
using DotNetBrightener.MultiTenancy.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.MultiTenancy.MiddleWares
{
    /// <summary>
    ///     The middleware that takes care of the routing for specific tenant,
    ///     will be triggered after the tenant of current request is identified
    /// </summary>
    public class TenantRoutingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger         _logger;

        public TenantRoutingMiddleware(RequestDelegate                  next,
                                       ILogger<TenantRoutingMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext              httpContext,
                                 ITenantPipelineContainer tenantPipelineContainer)
        {
            // picks the tenant context from current request
            var tenantContext = httpContext.Features.Get<TenantContext>();
            if (tenantContext != null)
            {
                // picks the pipeline from the container
                var pipeline = tenantPipelineContainer.GetPipeline(tenantContext.Tenant.Name);

                if (pipeline != null)
                {
                    _logger.LogInformation($"[{tenantContext.Tenant?.Name}] - Delegating request to tenant's pipeline");

                    await pipeline.Invoke(httpContext);

                    _logger.LogInformation($"[{tenantContext.Tenant?.Name}] - Request finished.");
                    return;
                }

                _logger.LogInformation($"[{tenantContext.Tenant?.Name}] - Marking tenant to restart in next request");

                // mark the shell context to be restarted in the next request, because the pipeline is not available
                tenantContext.NeedRestart = true;
            }

            await _next.Invoke(httpContext);
        }
    }
}