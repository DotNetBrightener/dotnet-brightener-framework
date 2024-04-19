using TimeZoneConverter;

namespace DotNetBrightener.Core.BackgroundTasks.Internal;

public static class TimeZoneConvert
{
    public static TimeZoneInfo ToTimeZoneInfo(this string timezoneIdOrIanaString)
    {
        if (string.IsNullOrEmpty(timezoneIdOrIanaString))
        {
            return TimeZoneInfo.Utc;
        }

        return TZConvert.GetTimeZoneInfo(timezoneIdOrIanaString);
    }
}