// ReSharper disable CheckNamespace

using System;
using DotNetBrightener.Core.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class LoggingEnableServiceCollectionExtensions
{
    public static IServiceCollection RegisterLoggingService<TServiceProvider>(this IServiceCollection serviceCollection)
        where TServiceProvider : IServiceProvider
    {
        serviceCollection.AddLogging();

        serviceCollection.AddScoped<IEventLogDataService, EventLogDataService>();
        serviceCollection.AddScoped<IQueueEventLogBackgroundProcessing, QueueEventLogBackgroundProcessing>();

        serviceCollection.AddSingleton<IEventLogWatcher>((provider) =>
        {
            var eventLogWatcher = EventLoggingWatcher.Instance;
            eventLogWatcher.SetServiceScopeFactory(provider.GetService<IServiceScopeFactory>()!);

            return eventLogWatcher;
        });

        return serviceCollection;
    }
}