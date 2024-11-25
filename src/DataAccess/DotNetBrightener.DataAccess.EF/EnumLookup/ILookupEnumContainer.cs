#nullable enable
using DotNetBrightener.DataAccess.EF.Converters;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.DataAccess.EF.EnumLookup;

public interface ILookupEnumContainer
{
    List<Action<ModelConfigurationBuilder>> ConventionConfigureActions { get; }

    void RegisterEnum<TEnum>() where TEnum : struct, Enum;
}

internal class LookupEnumContainer : ILookupEnumContainer
{
    public List<Action<ModelConfigurationBuilder>> ConventionConfigureActions { get; } = new();

    public void RegisterEnum<TEnum>() where TEnum : struct, Enum
    {
        ConventionConfigureActions.Add(builder =>
        {
            builder.Properties<TEnum>()
                   .HaveConversion<EnumLookupConverter<TEnum>>();
        });
    }
}