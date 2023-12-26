using Microsoft.AspNetCore.Authentication;

namespace DotNetBrightener.Infrastructure.ApiKeyAuthentication.Constants;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string HeaderName    = "x-api-key";
    public const string AuthenticationScheme = "XApiKeyScheme";
}