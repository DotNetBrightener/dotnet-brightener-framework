using System;

namespace DotNetBrightener.Core
{
    public interface IBackgroundServiceProvider : IServiceProvider
    {
    }

    public class BackgroundServiceProvider : IBackgroundServiceProvider
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
}