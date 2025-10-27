using System.Diagnostics;
using DotNetBrightener.Infrastructure.AppClientManager.Options;
using DotNetBrightener.Infrastructure.AppClientManager.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Web;

namespace DotNetBrightener.Infrastructure.AppClientManager.Middlewares;

public class AppClientCorsEnableMiddleware(
    RequestDelegate next,
    ICorsService    corsService)
{
    private static readonly CorsPolicy   PublicCorsPolicy = GetPublicCorsPolicy();

    public async Task Invoke(HttpContext                             context,
                             IOptions<CorsOptions> corsOptions,
                             IOptions<AppClientConfig>               appClientConfig,
                             IAppClientManager                       appClientManager,
                             IHttpContextAccessor                    httpContextAccessor,
                             IEnumerable<IAppBundleDetectionService> appBundleDetectionServices,
                             ILogger<AppClientCorsEnableMiddleware>  logger)
    {
        var option = appClientConfig.Value;

        var defaultPolicy = corsOptions.Value.GetPolicy(corsOptions.Value.DefaultPolicyName);

        if (option.OpenForPublicAccess)
        {
            var corsResult = corsService.EvaluatePolicy(context, PublicCorsPolicy);

            corsService.ApplyResult(corsResult, context.Response);

            var accessControlRequestMethod = context.Request.Headers[CorsConstants.AccessControlRequestMethod];

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

            return;
        }

        var result = await ProcessCorsBuilderForCurrentRequest(context,
                                                               appClientManager,
                                                               httpContextAccessor,
                                                               logger,
                                                               option,
                                                               defaultPolicy);

        foreach (var appBundleDetectionService in appBundleDetectionServices)
        {
            var appBundleId = appBundleDetectionService.GetBundleIdFromRequest(context);

            if (!string.IsNullOrEmpty(appBundleId))
            {
                result.RequestFromAppBundleId = appBundleId;

                logger.LogInformation("Request from App Bundle Id: {appBundleId}", appBundleId);

                break;
            }
        }


        if (!result.IsSuccess)
        {
            await next.Invoke(context);

            return;
        }

        if (result.ShortCircuit)
            return;

        await next.Invoke(context);
    }

    private async Task<AppClientIdentifyingResult> ProcessCorsBuilderForCurrentRequest(HttpContext context,
                                                                                       IAppClientManager
                                                                                           appClientManager,
                                                                                       IHttpContextAccessor
                                                                                           httpContextAccessor,
                                                                                       ILogger<
                                                                                               AppClientCorsEnableMiddleware>
                                                                                           logger,
                                                                                       AppClientConfig option,
                                                                                       CorsPolicy      defaultPolicy)
    {
        bool                        needCorsPolicyConfigured   = false;
        Uri?                        uriOrigin                  = null;
        StringValues                appClientId                = "";
        AppClientIdentifyingResult? appClientIdentifyingResult = new AppClientIdentifyingResult();

        // 1. Process for browser CORS first.
        if ((!context.Request
                     .Headers
                     .TryGetValue(CorsConstants.Origin, out var origin) ||
             !context.Request
                     .Headers
                     .TryGetValue("Referer", out origin)) &&
            !context.Request
                    .Headers
                    .TryGetValue(option.ClientIdHeaderKey,
                                 out appClientId))
        {
            logger.LogInformation("No [origin] or [{clientId}] header found in the request. " +
                                  "CORS will not be enabled for this request.",
                                  option.ClientIdHeaderKey);

            return appClientIdentifyingResult;
        }

        if (string.IsNullOrEmpty(origin) &&
            string.IsNullOrEmpty(appClientId))
        {
            logger.LogInformation("No [origin] or [{clientId}] header found in the request. " +
                                  "CORS will not be enabled for this request.",
                                  option.ClientIdHeaderKey);

            return appClientIdentifyingResult;
        }

        string requestDomainName = "";

        if (!string.IsNullOrEmpty(origin))
        {
            uriOrigin                = new Uri(origin.ToString());
            requestDomainName        = uriOrigin.GetDomain();
            needCorsPolicyConfigured = true;
        }

        var requestFromAppClientId = appClientId.ToString();

        var associatedApp = !string.IsNullOrEmpty(requestFromAppClientId)
                                ? await appClientManager.GetClientByClientId(requestFromAppClientId)
                                : await appClientManager.GetClientByHostNameOrByBundleId(requestDomainName);

        if (associatedApp is null)
        {
            return appClientIdentifyingResult;
        }

        if (!string.IsNullOrEmpty(requestFromAppClientId) &&
            !string.IsNullOrEmpty(requestDomainName))
        {
            if (associatedApp.AllowedOrigins is null ||
                associatedApp.AllowedOrigins != "*" ||
                !associatedApp.AllowedOrigins.Contains(requestDomainName))
            {
                return appClientIdentifyingResult;
            }
        }

        appClientIdentifyingResult.RequestFromAppDomain   = requestDomainName;
        appClientIdentifyingResult.RequestFromAppClientId = requestFromAppClientId;

        appClientIdentifyingResult.Success(associatedApp, needCorsPolicyConfigured);

        httpContextAccessor.StoreValue(associatedApp);
        httpContextAccessor.StoreValue(appClientIdentifyingResult);

        if (uriOrigin is null)
        {
            return appClientIdentifyingResult;
        }

        var shouldShortCircuit = BuildCorsPolicyAndApplyForUrl(context, uriOrigin, defaultPolicy);

        appClientIdentifyingResult.ShortCircuit = shouldShortCircuit;

        return appClientIdentifyingResult;
    }

    /// <summary>
    ///     Build the CORS for the given URL, and apply it to the response
    /// </summary>
    /// <param name="context"></param>
    /// <param name="uriOrigin"></param>
    /// <param name="defaultPolicy"></param>
    private bool BuildCorsPolicyAndApplyForUrl(HttpContext context, Uri uriOrigin, CorsPolicy defaultPolicy)
    {
        var policyBuilder = new CorsPolicyBuilder();

        policyBuilder.WithOrigins(uriOrigin.GetBaseUrl())
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();

        if (defaultPolicy is { ExposedHeaders.Count: > 0 })
        {
            policyBuilder.WithExposedHeaders(defaultPolicy.ExposedHeaders.ToArray());
        }

        var corsPolicy = policyBuilder.Build();

        var corsResult = corsService.EvaluatePolicy(context, corsPolicy);

        corsService.ApplyResult(corsResult, context.Response);

        var accessControlRequestMethod = context.Request.Headers[CorsConstants.AccessControlRequestMethod];

        if (string.Equals(context.Request.Method,
                          CorsConstants.PreflightHttpMethod,
                          StringComparison.Ordinal) &&
            !StringValues.IsNullOrEmpty(accessControlRequestMethod))
        {
            // Since there is a policy which was identified,
            // always respond to preflight requests.
            context.Response.StatusCode = StatusCodes.Status200OK;

            return true;
        }

        return false;
    }

    internal static CorsPolicy GetPublicCorsPolicy()
    {
        var policyBuilder = new CorsPolicyBuilder();

        policyBuilder.AllowAnyOrigin()
                     .AllowAnyHeader()
                     .AllowAnyMethod();

        var corsPolicy = policyBuilder.Build();

        return corsPolicy;
    }
}
