using DotNetBrightener.Infrastructure.AppClientManager.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Infrastructure.AppClientManager.Services;

public interface IAppBundleDetectionService
{
    string? GetBundleIdFromRequest(HttpContext context);
}

public class UserAgentBasedAppBundleDetectionService : IAppBundleDetectionService
{
    public string? GetBundleIdFromRequest(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.ToString();

        var appId = userAgent.Split(";", StringSplitOptions.RemoveEmptyEntries)
                             .FirstOrDefault(_ => _.StartsWith("AppId", StringComparison.OrdinalIgnoreCase))
                          ?
                         .Replace("appid=", "", StringComparison.OrdinalIgnoreCase);

        return appId;
    }
}

public class HttpHeaderBaseAppBundleDetectionService : IAppBundleDetectionService
{
    private readonly AppClientConfig _options;

    public HttpHeaderBaseAppBundleDetectionService(IOptions<AppClientConfig> options)
    {
        _options = options.Value;
    }

    public string? GetBundleIdFromRequest(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(_options.ClientAppBundleIdHeaderKey, out var appBundleId))
            return appBundleId.ToString();

        return null;
    }
}