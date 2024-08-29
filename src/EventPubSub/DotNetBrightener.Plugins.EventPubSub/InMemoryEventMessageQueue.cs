using System.Diagnostics;
using System.Threading.Channels;

namespace DotNetBrightener.Plugins.EventPubSub;

internal class InMemoryEventMessageQueue
{
    private readonly Channel<IEventMessage> _channel = Channel.CreateUnbounded<IEventMessage>();

    [DebuggerStepThrough]
    public async Task Enqueue(IEventMessage eventMessage)
    {
        await _channel.Writer.WriteAsync(eventMessage);
    }

    [DebuggerStepThrough]
    public IAsyncEnumerable<IEventMessage> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}