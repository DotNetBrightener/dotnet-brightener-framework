using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using DotNetBrightener.SiteSettings.Models;

namespace DotNetBrightener.SiteSettings;

internal class SiteSettingsContractResolver : DefaultContractResolver
{
    private static readonly List<string> IgnoreProperties =
    [
        nameof(SiteSettingBase.SettingName),
        nameof(SiteSettingBase.Description),
    ];

    public static SiteSettingsContractResolver Instance { get; } = new();

    private SiteSettingsContractResolver()
    {
        NamingStrategy = new CamelCaseNamingStrategy();
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        Predicate<object> propertyShouldSerialize = instance =>
        {
            var propertyPropertyName = property.PropertyName!;

            var shouldSerialize = !IgnoreProperties.Contains(propertyPropertyName) &&
                                  !IgnoreProperties.Contains(propertyPropertyName[0].ToString().ToUpper() +
                                                             propertyPropertyName.Substring(1));

            return shouldSerialize;
        };

        property.ShouldSerialize = propertyShouldSerialize;

        return property;
    }
}