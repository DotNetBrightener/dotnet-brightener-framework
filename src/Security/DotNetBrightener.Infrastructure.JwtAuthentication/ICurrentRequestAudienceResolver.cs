using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public interface ICurrentRequestAudienceResolver
{
    string[] GetAudiences();
}

internal class CurrentRequestAudienceResolver(
    IHttpContextAccessor                    httpContextAccessor,
    ILogger<CurrentRequestAudienceResolver> logger) : ICurrentRequestAudienceResolver
{
    public string[] GetAudiences()
    {
        var referer = httpContextAccessor.HttpContext?.Request.Headers.Origin.ToString();

        logger.LogDebug("Request Origin: {origin}", referer);

        if (!string.IsNullOrEmpty(referer))
        {
            logger.LogDebug("Expecting audience from current request: {audience}", referer);
            return [referer];
        }

        referer = httpContextAccessor.HttpContext?.Request.Headers.Referer.ToString();

        logger.LogDebug("Request Referer: {referer}", referer);

        if (string.IsNullOrEmpty(referer))
        {
            return [];
        }

        var refererUrl = new Uri(new Uri(referer), "/").ToString();

        logger.LogDebug("Expecting audience from current request: {audience}", refererUrl);
        
        return
        [
            refererUrl
        ];
    }
}