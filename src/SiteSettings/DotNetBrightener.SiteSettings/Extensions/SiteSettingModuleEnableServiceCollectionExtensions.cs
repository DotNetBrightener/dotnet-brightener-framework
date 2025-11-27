using DotNetBrightener.SiteSettings.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.SiteSettings.Extensions;

public static class SiteSettingModuleEnableServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection RegisterSettingType<TSettingType>()
            where TSettingType : SiteSettingBase, new()
        {
            serviceCollection.AddSingleton<SiteSettingBase, TSettingType>();
            serviceCollection.AddSingleton<TSettingType, TSettingType>();

            return serviceCollection;
        }
    }
}