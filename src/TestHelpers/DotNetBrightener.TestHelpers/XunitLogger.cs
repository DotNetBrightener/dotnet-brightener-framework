using Microsoft.Extensions.Logging;

namespace DotNetBrightener.TestHelpers;

internal class XunitLogger : ILogger
{
    private          LogQueue _logQueue;
    private readonly string   _categoryName;

    public XunitLogger(LogQueue logQueue, string categoryName)
    {
        _logQueue = logQueue;
        _categoryName  = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message)) return;

        _logQueue.Enqueue($"{logLevel}: {_categoryName}: {message}").Wait();

        if (exception != null)
        {
            _logQueue.Enqueue(exception.ToString()).Wait();
        }
    }
}