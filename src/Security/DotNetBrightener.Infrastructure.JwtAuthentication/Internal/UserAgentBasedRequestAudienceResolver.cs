using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.Infrastructure.JwtAuthentication.Internal;

internal class UserAgentBasedRequestAudienceResolver(IHttpContextAccessor httpContextAccessor)
    : ICurrentRequestAudienceResolver
{
    public string[] GetAudiences()
    {
        var userAgent = httpContextAccessor.HttpContext?.Request
                                           .Headers
                                           .UserAgent.ToString();

        if (!string.IsNullOrEmpty(userAgent) &&
            userAgent.Contains("AppId=", StringComparison.OrdinalIgnoreCase))
        {
            var appId = userAgent.Split(";", StringSplitOptions.RemoveEmptyEntries)
                                 .Where(ua => ua.StartsWith("AppId=", StringComparison.OrdinalIgnoreCase))
                                 .Select(st => st.Replace("AppId=", "", StringComparison.OrdinalIgnoreCase))
                                 .ToArray();

            return [..appId];
        }

        if (!string.IsNullOrEmpty(userAgent) &&
            userAgent.Contains("AppBundleId=", StringComparison.OrdinalIgnoreCase))
        {
            var appId = userAgent.Split(";", StringSplitOptions.RemoveEmptyEntries)
                                 .Where(ua => ua.StartsWith("AppBundleId=", StringComparison.OrdinalIgnoreCase))
                                 .Select(st => st.Replace("AppBundleId=", "", StringComparison.OrdinalIgnoreCase))
                                 .ToArray();

            return [..appId];
        }

        return [];
    }
}