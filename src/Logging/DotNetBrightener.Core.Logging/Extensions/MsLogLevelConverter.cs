using NLog;

namespace Microsoft.Extensions.DependencyInjection;

internal static class MsLogLevelConverter
{
    public static LogLevel ToNLogLevel(this Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        return logLevel switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace       => LogLevel.Trace,
            Microsoft.Extensions.Logging.LogLevel.Debug       => LogLevel.Debug,
            Microsoft.Extensions.Logging.LogLevel.Information => LogLevel.Info,
            Microsoft.Extensions.Logging.LogLevel.Warning     => LogLevel.Warn,
            Microsoft.Extensions.Logging.LogLevel.Error       => LogLevel.Error,
            Microsoft.Extensions.Logging.LogLevel.Critical    => LogLevel.Fatal,
            _                                                 => LogLevel.Info
        };
    }
}