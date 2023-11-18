using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace DotNetBrightener.DataAccess.EF.Converters;

public class DateOnlyConverter : ValueConverter<DateOnly, DateTime>
{
    public DateOnlyConverter()
        : base(
               dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
               dateTime => DateOnly.FromDateTime(dateTime))
    {
    }
}