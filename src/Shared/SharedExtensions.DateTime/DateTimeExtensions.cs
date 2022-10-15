namespace System;

internal static class DateTimeExtensions
{
    public static double GetUnixTimestamp(this DateTime dateTimeValue, DateTimeKind dateTimeKind = DateTimeKind.Utc)
    {
        if (dateTimeKind != DateTimeKind.Utc)
        {
            dateTimeValue = dateTimeValue.ToUniversalTime();
        }

        return dateTimeValue.Subtract(DateTime.UnixEpoch).TotalMilliseconds;
    }
}