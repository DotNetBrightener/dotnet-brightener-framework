#nullable enable
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Plugins.EventPubSub.Distributed;

public interface IDistributedEventPubSubConfigurator
{
    IServiceCollection Services { get; }

    EventPubSubServiceBuilder Builder { get; }

    string AppName { get; }
}


internal class DistributedIntegrationsConfigurator : IDistributedEventPubSubConfigurator
{
    public IServiceCollection Services { get; init; }

    public EventPubSubServiceBuilder Builder { get; init; }

    public string AppName { get; internal set; }

    public IEndpointNameFormatter NameFormatter { get; internal set; } = KebabCaseEndpointNameFormatter.Instance;

    public IEntityNameFormatter? EntityNameFormatter { get; internal set; } = null;
    
    public List<Action<IBusRegistrationConfigurator, Type[]>> ConfigureTransports { get; init; } = new();
}