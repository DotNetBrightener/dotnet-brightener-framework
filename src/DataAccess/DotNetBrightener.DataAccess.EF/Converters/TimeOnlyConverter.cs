using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotNetBrightener.DataAccess.EF.Converters;

public class TimeOnlyConverter() : ValueConverter<TimeOnly, TimeSpan>(timeOnly => timeOnly.ToTimeSpan(),
                                                                      timeSpan => TimeOnly.FromTimeSpan(timeSpan));