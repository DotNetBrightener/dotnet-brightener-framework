using DotNetBrightener.OAuth.DependenciesInjection;
using DotNetBrightener.OAuth.Integration.Google.Providers;
using DotNetBrightener.OAuth.Integration.Google.Settings;
using DotNetBrightener.OAuth.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.OAuth.Integration.Google.Extensions;

public static class GoogleOAuthServiceCollectionExtensions
{
    public static IServiceCollection AddGoogleAuthentication(this IServiceCollection services,
                                                             IConfiguration configuration)
    {
        services.AddOAuthProvider<GoogleOAuthServiceProvider>()
                .AddOAuthSettingLoader<GoogleOAuthSettings, DefaultOAuthProviderSettingLoader<GoogleOAuthSettings>>();

        services.Configure<GoogleOAuthSettings>(configuration.GetSection(nameof(GoogleOAuthSettings)));

        return services;
    }
}