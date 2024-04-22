using DotNetBrightener.SiteSettings.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.SiteSettings.Extensions;

public static class SiteSettingModuleEnableServiceCollectionExtensions
{
    public static IServiceCollection RegisterSettingType<TSettingType>(this IServiceCollection serviceCollection)
        where TSettingType : SiteSettingBase, new()
    {
        serviceCollection.AddSingleton<SiteSettingBase, TSettingType>();
        serviceCollection.AddSingleton<TSettingType, TSettingType>();

        return serviceCollection;
    }
}