using System;
using TimeZoneConverter;

namespace DotNetBrightener.CommonShared.Services;

public interface ITimezoneHandler : IDependency
{
    TimeZoneInfo GetTimezoneFromString(string timezoneIdOrIanaString);
}

public class DefaultTimezoneHandler : ITimezoneHandler
{
    public TimeZoneInfo GetTimezoneFromString(string timezoneIdOrIanaString)
    {
        if (string.IsNullOrEmpty(timezoneIdOrIanaString))
        {
            return TimeZoneInfo.Utc;
        }

        return TZConvert.GetTimeZoneInfo(timezoneIdOrIanaString);
    }
}