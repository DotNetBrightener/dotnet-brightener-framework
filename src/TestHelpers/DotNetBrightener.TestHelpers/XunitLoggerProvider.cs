using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Channels;
using Xunit.Abstractions;

namespace DotNetBrightener.TestHelpers;

public class XunitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly LogQueue          _logQueue = new LogQueue();

    public XunitLoggerProvider(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(_logQueue, categoryName);
    }

    private async Task ProcessLogEntries()
    {
        await foreach (var eventMessage in _logQueue.ReadAllAsync())
        {
            _outputHelper.WriteLine(eventMessage);
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