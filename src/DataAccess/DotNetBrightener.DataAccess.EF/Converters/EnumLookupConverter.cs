using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotNetBrightener.DataAccess.EF.Converters;

public class EnumLookupConverter<TEnum> : ValueConverter<TEnum, int> where TEnum : struct, Enum
{
    public EnumLookupConverter()
        : base(
               enumValue => Convert.ToInt32(enumValue),
               intValue => (TEnum)Enum.ToObject(typeof(TEnum), intValue))
    {
    }
}