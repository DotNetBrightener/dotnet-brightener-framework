using DotNetBrightener.Core.BackgroundTasks;
using DotNetBrightener.Core.BackgroundTasks.HostedServices;
using DotNetBrightener.Core.BackgroundTasks.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    public static IServiceCollection EnableBackgroundTaskServices(this IServiceCollection services,
                                                                  IConfiguration configuration)
    {
        // option
        services.Configure<BackgroundTaskOptions>(configuration.GetSection(nameof(BackgroundTaskOptions)));

        // Background tasks
        services.AddSingleton<IBackgroundTaskScheduler, BackgroundTaskScheduler>();

        services.AddScoped<IBackgroundTaskRunner, BackgroundTaskRunner>();
        services.AddHostedService<SchedulerHostedService>();

        services.AddSingleton<IScheduler, Scheduler>();
        services.AddHostedService<BackgroundTaskHostService>();

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
        services.TryAddScoped<TBackgroundTask, TBackgroundTask>();

        return services;
    }
}