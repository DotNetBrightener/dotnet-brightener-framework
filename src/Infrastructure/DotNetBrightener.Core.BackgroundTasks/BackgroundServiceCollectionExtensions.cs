using DotNetBrightener.Core.BackgroundTasks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class BackgroundServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the required services for background tasks into the given <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <returns>
    ///     The same instance of <paramref name="services" /> for chaining operations
    /// </returns>
    public static IServiceCollection EnableBackgroundTaskServices(this IServiceCollection services)
    {
        // Background tasks
        services.AddSingleton<IBackgroundServiceProvider>(provider => new BackgroundServiceProvider(provider));
        services.AddSingleton<IBackgroundTaskContainerService, BackgroundTaskContainerService>();
        services.AddSingleton<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
        services.AddScoped<IBackgroundTaskRunner, BackgroundTaskRunner>();

        return services;
    }

    /// <summary>
    ///     Registers a background task implementation of type <typeparamref name="TBackgroundTask" /> into the <see cref="IServiceCollection" />
    /// </summary>
    /// <typeparam name="TBackgroundTask">The type of the background task implements the <see cref="IBackgroundTask" /></typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <returns>
    ///     The same instance of <paramref name="services" /> for chaining operations
    /// </returns>
    public static IServiceCollection AddBackgroundTask<TBackgroundTask>(this IServiceCollection services) 
        where TBackgroundTask : class, IBackgroundTask
    {
        services.AddScoped<IBackgroundTask, TBackgroundTask>();

        return services;
    }
}