using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotNetBrightener.DataAccess.EF.Converters;

public class DateTimeOffsetUtcConverter()
    : ValueConverter<DateTimeOffset, DateTimeOffset>(saving => saving.ToUniversalTime(),
                                                     fetching => fetching);