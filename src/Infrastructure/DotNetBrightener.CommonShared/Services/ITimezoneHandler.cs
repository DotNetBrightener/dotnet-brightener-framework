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
        return TZConvert.GetTimeZoneInfo(timezoneIdOrIanaString);
    }
}