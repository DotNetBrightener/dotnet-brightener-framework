using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public interface ICurrentRequestAudienceResolver
{
    string[] GetAudiences();
}

internal class NullCurrentRequestAudienceResolver(IHttpContextAccessor httpContextAccessor) : ICurrentRequestAudienceResolver
{
    public string[] GetAudiences()
    {
        var referer = httpContextAccessor.HttpContext?.Request.Headers.Referer.ToString();

        if (string.IsNullOrEmpty(referer))
        {
            return [];
        }

        var refererUrl = new Uri(new Uri(referer), "/").ToString();

        return
        [
            refererUrl
        ];
    }
}