using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Plugins.EventPubSub;

public class EventPubSubServiceBuilder
{
    public IServiceCollection Services { get; init; }

    public IDistributedMessengerBuilder DistributedMessengerBuilder { get; set; }

    public Type[] EventMessageTypes { get; internal set; }
}

public interface IDistributedMessengerBuilder
{
    Action<IServiceCollection> ConfigureServices { get; }
}