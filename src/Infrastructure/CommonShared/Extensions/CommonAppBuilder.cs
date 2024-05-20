using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.CommonShared;

public class CommonAppBuilder
{
    public IServiceCollection Services { get; init; }
    public EventPubSubServiceBuilder EventPubSubServiceBuilder { get; init; }
}