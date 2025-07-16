using DotNetBrightener.Infrastructure.JwtAuthentication;
using Microsoft.Extensions.Configuration;

namespace WebApp.CommonShared.Internal;

internal class DefaultAllowedOriginsAudienceValidator(IConfiguration configuration) : IAuthAudienceValidator
{
    public string[] GetValidAudiences()
    {
        return configuration.GetDefaultAllowedOrigins();
    }
}