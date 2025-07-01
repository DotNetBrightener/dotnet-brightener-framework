using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TestHelpers.PostgreSql;

internal class XunitLogger(LogQueue logQueue, string categoryName) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message)) return;

        logQueue.Enqueue($"{logLevel}: {categoryName}: {message}").Wait();

        if (exception != null)
        {
            logQueue.Enqueue(exception.ToString()).Wait();
        }
    }
}