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

    public static double GetUnixTimestampInSeconds(this DateTime dateTimeValue)
    {
        if (dateTimeValue.Kind != DateTimeKind.Utc)
        {
            dateTimeValue = dateTimeValue.ToUniversalTime();
        }

        return dateTimeValue.Subtract(DateTime.UnixEpoch).TotalSeconds;
    }
}