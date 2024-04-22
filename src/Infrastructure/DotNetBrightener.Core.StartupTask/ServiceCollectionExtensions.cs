// ReSharper disable CheckNamespace
using DotNetBrightener.Core.StartupTask;
using DotNetBrightener.Core.StartupTask.StartupServices;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private static bool _hasHostedService = false;

    /// <summary>
    ///     Registers a service that is executed at the application startup to the service collection
    /// </summary>
    /// <typeparam name="T">
    ///     Type of the startup task service, derived from <see cref="IStartupTask"/>
    /// </typeparam>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    public static IServiceCollection RegisterStartupTask<T>(this IServiceCollection serviceCollection)
        where T : class, IStartupTask
    {
        serviceCollection.AddScoped<IStartupTask, T>();
        serviceCollection.AddScoped<T, T>();

        if (!_hasHostedService)
        {
            AddStartupTasksService(serviceCollection);
        }

        return serviceCollection;
    }

    public static IServiceCollection AddStartupTasksService(this IServiceCollection serviceCollection)
    {
        if (!_hasHostedService)
        {
            serviceCollection.AddHostedService<StartupTaskExecutionHostedService>();
            _hasHostedService = true;
        }

        serviceCollection.TryAddSingleton<IServiceCollection>(serviceCollection);

        return serviceCollection;
    }
}