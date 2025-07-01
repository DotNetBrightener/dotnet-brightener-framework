using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DotNetBrightener.TestHelpers.PostgreSql;

public class XunitLoggerProvider(ITestOutputHelper outputHelper) : ILoggerProvider
{
    private readonly LogQueue          _logQueue     = new();

    public ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(_logQueue, categoryName);
    }

    private async Task ProcessLogEntries()
    {
        await foreach (var eventMessage in _logQueue.ReadAllAsync())
        {
            outputHelper.WriteLine(eventMessage);
        }
    }

    public void Dispose()
    {
        ProcessLogEntries().Wait();
    }
}


internal class LogQueue
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();

    [DebuggerStepThrough]
    public async Task Enqueue(string eventMessage)
    {
        await _channel.Writer.WriteAsync(eventMessage);
    }

    [DebuggerStepThrough]
    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}