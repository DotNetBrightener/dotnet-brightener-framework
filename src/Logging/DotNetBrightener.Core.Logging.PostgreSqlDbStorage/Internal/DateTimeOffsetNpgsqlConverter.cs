using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotNetBrightener.Core.Logging.PostgreSqlDbStorage.Internal;

internal class DateTimeOffsetNpgsqlConverter()
    : ValueConverter<DateTimeOffset, DateTimeOffset>(saving => saving.ToUniversalTime(),
                                                     fetching =>
                                                         DateTime.SpecifyKind(fetching.DateTime, DateTimeKind.Utc));