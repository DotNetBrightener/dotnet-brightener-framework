using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Plugins.EventPubSub;

public class EventPubSubServiceBuilder
{
    public IServiceCollection Services { get; init; }

    public List<Type> EventMessageTypes { get; } = [];
}