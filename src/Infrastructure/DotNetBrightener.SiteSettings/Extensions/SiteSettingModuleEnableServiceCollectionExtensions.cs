using System;
using System.Reflection;
using DotNetBrightener.SiteSettings.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace DotNetBrightener.SiteSettings.Extensions;

public static class SiteSettingModuleEnableServiceCollectionExtensions
{
    public static IServiceCollection RegisterSettingType<TSettingType>(this IServiceCollection serviceCollection,
                                                                       Action<TSettingType> settingDefaultValue =
                                                                           null)
        where TSettingType : SiteSettingBase, new()
    {
        object SettingInstanceFactory(IServiceProvider provider)
        {
            var localizerFactory = provider.GetService<IStringLocalizerFactory>();

            var localizer = localizerFactory.Create(typeof(TSettingType));

            var settingInstance = provider.TryGetService<TSettingType>();

            var localizerMember =
                typeof(SiteSettingBase).GetProperty("T",
                                                    BindingFlags.Instance |
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.Public |
                                                    BindingFlags.Default);

            if (localizerMember != null &&
                localizerMember.PropertyType == typeof(IStringLocalizer))
            {
                localizerMember.SetValue(settingInstance, localizer);
            }

            if (settingDefaultValue != null)
            {
                settingDefaultValue.Invoke(settingInstance as TSettingType);
            }

            return settingInstance;
        }

        serviceCollection.AddTransient(typeof(SiteSettingBase), SettingInstanceFactory);
        serviceCollection.AddTransient(typeof(TSettingType), SettingInstanceFactory);

        return serviceCollection;
    }
}