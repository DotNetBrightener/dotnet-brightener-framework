// ReSharper disable CheckNamespace
using DotNetBrightener.Core.StartupTask;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers a service that is executed at the application startup to the service collection
    /// </summary>
    /// <typeparam name="T">Type of the startup task service, derived from <see cref="IStartupTask"/></typeparam>
    /// <param name="serviceCollection">
    ///     The <see cref="IServiceCollection"/>
    /// </param>
    public static void RegisterStartupTask<T>(this IServiceCollection serviceCollection)
        where T : class, IStartupTask
    {
        serviceCollection.AddScoped<IStartupTask, T>();
    }

    /// <summary>
    ///     Executes the startup tasks from the given service provider
    /// </summary>
    /// <param name="serviceProvider">
    ///     The <see cref="IServiceProvider"/> to extract the startup tasks and execute them
    /// </param>
    /// <returns></returns>
    public static async Task ExecuteStartupTasks(this IServiceProvider serviceProvider)
    {
        Type[]                startupTaskTypes;

        using (var serviceScope = serviceProvider.CreateScope())
        {
            startupTaskTypes = serviceScope.ServiceProvider
                                           .GetServices<IStartupTask>()
                                           .OrderBy(_ => _.Order)
                                           .Select(_ => _.GetType())
                                           .ToArray();
        }

        var logger = serviceProvider.GetService<ILogger<IStartupTask>>();

        if (startupTaskTypes.Length == 0)
        {
            logger?.LogDebug("No start up tasks found.");

            return;
        }

        logger?.LogInformation("Found {tasks} start up tasks: {tasksList}...",
                               startupTaskTypes.Length,
                               string.Join(", ", startupTaskTypes.Select(taskType => taskType.Name)));

        var synchronousTasks = startupTaskTypes
                              .Where(taskType => taskType.IsAssignableTo(typeof(ISynchronousStartupTask)))
                              .ToArray();

        var asynchronousTasks = startupTaskTypes.Except(synchronousTasks)
                                                .ToArray();

        Stopwatch sw = Stopwatch.StartNew();

        foreach (var startupTaskType in synchronousTasks)
        {
            await ExecuteTask(serviceProvider, startupTaskType);
        }


        await Parallel.ForEachAsync(asynchronousTasks,
                                    async (type, cancellationToken) =>
                                    {
                                        await ExecuteTask(serviceProvider, type);
                                    });

        sw.Stop();

        logger?.LogInformation("Start up tasks execution finished after {Elapsed}. " +
                               "Total {numberOfTasks} have executed.",
                               sw.Elapsed,
                               startupTaskTypes.Length);
    }

    private static async Task ExecuteTask(IServiceProvider serviceProvider,
                                          Type             startupTaskType)
    {
        var       taskType            = startupTaskType.Name;
        using var backgroundTaskScope = serviceProvider.CreateScope();

        ILogger<IStartupTask> subLogger = backgroundTaskScope.ServiceProvider
                                                             .GetService<ILogger<IStartupTask>>();

        if (backgroundTaskScope.ServiceProvider
                               .TryGet(startupTaskType) is not IStartupTask taskInstance)
        {
            subLogger.LogWarning("Cannot resolve task {taskType} from the service collection. Skipping...",
                                 taskType);

            return;
        }


        subLogger.LogInformation("Starting task {taskType} execution {syncState}...",
                                 taskType,
                                 taskInstance is ISynchronousStartupTask
                                     ? "synchronously"
                                     : "asynchronously");

        Stopwatch sw = Stopwatch.StartNew();

        await taskInstance.Execute();

        sw.Stop();

        subLogger.LogInformation("Finished task {taskType} execution after {Elapsed}.", taskType, sw.Elapsed);
    }
}