using DotNetBrightener.OAuth.DependenciesInjection;
using DotNetBrightener.OAuth.Integration.Apple.Providers;
using DotNetBrightener.OAuth.Integration.Apple.Settings;
using DotNetBrightener.OAuth.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.OAuth.Integration.Apple.Extensions;

public static class AppleOAuthServiceCollectionExtensions
{
    public static IServiceCollection AddAppleAuthentication(this IServiceCollection services,
                                                            IConfiguration          configuration)
    {
        services.AddOAuthProvider<AppleOAuthServiceProvider>()
                .AddOAuthSettingLoader<AppleOAuthSettings, DefaultOAuthProviderSettingLoader<AppleOAuthSettings>>();

        services.Configure<AppleOAuthSettings>(configuration.GetSection(nameof(AppleOAuthSettings)));

        return services;
    }
}