using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotNetBrightener.DataAccess.EF.Converters;

public class EnumLookupConverter<TEnum>() : ValueConverter<TEnum, int>(enumValue => Convert.ToInt32(enumValue),
                                                                       intValue =>
                                                                           (TEnum)Enum.ToObject(typeof(TEnum),
                                                                                                intValue))
    where TEnum : struct, Enum;