using System;

namespace DotNetBrightener.Plugins.EventPubSub.DependencyInjection.Internal;

internal class BackgroundServiceProvider : IEventPubSubBackgroundServiceProvider
{
    private readonly IServiceProvider _serviceProvider;

    public BackgroundServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object GetService(Type serviceType)
    {
        return _serviceProvider.GetService(serviceType);
    }
}