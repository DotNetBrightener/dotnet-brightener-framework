using DotNetBrightener.OAuth.Providers;
using DotNetBrightener.OAuth.Services;
using DotNetBrightener.OAuth.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.OAuth.DependenciesInjection;

public static class OAuthServiceCollectionExtensions
{
    /// <summary>
    ///     Registers services for OAuth service providers
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configAuthSettings"></param>
    /// <returns></returns>
    public static IServiceCollection AddOAuthServices(this IServiceCollection services,
                                                      Action<OAuthSettings> configAuthSettings = null)
    {
        services.AddSingleton<IOAuthRequestManager, OAuthRequestManager>();
        services.Configure<OAuthSettings>((instance) =>
        {
            configAuthSettings?.Invoke(instance);
        });

        return services;
    }

    /// <summary>
    ///     Registers settings loader of given <typeparamref name="TSettingType"/>
    /// </summary>
    /// <typeparam name="TSettingType">The of OAuth Provider Settings</typeparam>
    /// <typeparam name="TSettingImplementation"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddOAuthSettingLoader<TSettingType, TSettingImplementation>(
        this IServiceCollection services)
        where TSettingType : class, IOAuthProviderSetting
        where TSettingImplementation : class, IOAuthProviderSettingLoader<TSettingType>
    {
        var existingRegistration = services.FirstOrDefault(_ => _.ServiceType == typeof(IOAuthProviderSettingLoader<TSettingType>));

        if (existingRegistration is not null)
            services.RemoveAll<IOAuthProviderSettingLoader<TSettingType>>();

        services.AddScoped<IOAuthProviderSettingLoader<TSettingType>, TSettingImplementation>();

        return services;
    }

    public static IServiceCollection AddOAuthProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IOAuthServiceProvider
    {
        services.AddScoped<IOAuthServiceProvider, TProvider>();

        return services;
    }
}
