using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotNetBrightener.DataAccess.EF.Converters;

public class DateOnlyConverter()
    : ValueConverter<DateOnly, DateTime>(dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
                                         dateTime => DateOnly.FromDateTime(dateTime));