using DotNetBrightener.Infrastructure.JwtAuthentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;

namespace WebApp.CommonShared.Internal;

internal class DefaultAllowedOriginAudienceResolver(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration       configuration)
    : ICurrentRequestAudienceResolver
{
    public string[] GetAudiences()
    {
        var allowedOrigins = configuration.GetDefaultAllowedOrigins();

        var requestUrl = httpContextAccessor.HttpContext?.Request.GetDisplayUrl();

        if (requestUrl is null ||
            !allowedOrigins.Any())
            return [];

        // Extract the origin from the request URL
        var origin = new Uri(new Uri(requestUrl), "/").ToString()
                                                      .Trim('/');


        // Add the request origin if it's among allowed origins
        if (allowedOrigins.Contains(origin))
        {
            return [origin];
        }

        return [];
    }
}