using System;

namespace DotNetBrightener.Core.BackgroundTasks;

public interface IBackgroundServiceProvider : IServiceProvider { }

internal class BackgroundServiceProvider : IBackgroundServiceProvider
{
    private readonly IServiceProvider _serviceProvider;

    public BackgroundServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object GetService(Type serviceType)
    {
        return _serviceProvider.TryGetService(serviceType);
    }
}