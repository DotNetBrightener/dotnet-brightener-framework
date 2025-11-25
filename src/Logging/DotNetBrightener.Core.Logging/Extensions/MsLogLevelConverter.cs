using NLog;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal static class MsLogLevelConverter
{
    public static LogLevel ToNLogLevel(this Microsoft.Extensions.Logging.LogLevel logLevel)
    {
        return logLevel switch
        {
            Logging.LogLevel.Trace       => LogLevel.Trace,
            Logging.LogLevel.Debug       => LogLevel.Debug,
            Logging.LogLevel.Information => LogLevel.Info,
            Logging.LogLevel.Warning     => LogLevel.Warn,
            Logging.LogLevel.Error       => LogLevel.Error,
            Logging.LogLevel.Critical    => LogLevel.Fatal,
            _                            => LogLevel.Info
        };
    }
}