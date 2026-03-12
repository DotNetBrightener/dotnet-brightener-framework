using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DotNetBrightener.Core.StartupTask.StartupServices;

internal class StartupTaskExecutionHostedService(
    IServiceScopeFactory                       serviceScopeFactory,
    ILogger<StartupTaskExecutionHostedService> logger,
    IHostApplicationLifetime                   lifetime)
    : IHostedService, IDisposable
{
    private int _attempts;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        lifetime.ApplicationStarted.Register(async () =>
        {
            await Execute();
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
    }

    public void Dispose()
    {
    }

    private async Task Execute()
    {
        while (_attempts < 3)
        {
            try
            {
                await ExecuteStartupTasks(logger);

                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                                 "Failed to resolve start up tasks after {attempts} attempts. Retrying...",
                                 _attempts);
                _attempts++;
            }
        }
    }

    private async Task ExecuteStartupTasks(ILogger logger)
    {
        Type[] startupTaskTypes = [];

        try
        {
            using (var serviceScope = serviceScopeFactory.CreateScope())
            {
                startupTaskTypes = serviceScope.ServiceProvider
                                               .GetServices<IStartupTask>()
                                               .OrderBy(_ => _.Order)
                                               .Select(_ => _.GetType())
                                               .ToArray();
            }
        }
        catch (InvalidOperationException ex)
        {
            Interlocked.Increment(ref _attempts);

            throw;
        }

        if (startupTaskTypes.Length == 0)
        {
            logger.LogDebug("No start up tasks found.");

            return;
        }

        logger.LogInformation("Found {tasks} start up tasks: \r\n\t - {tasksList}",
                              startupTaskTypes.Length,
                              string.Join("\r\n\t - ", startupTaskTypes.Select(taskType => taskType.Name)));

        var synchronousTasks = startupTaskTypes
                              .Where(taskType => taskType.IsAssignableTo(typeof(ISynchronousStartupTask)))
                              .ToArray();

        var asynchronousTasks = startupTaskTypes.Except(synchronousTasks)
                                                .ToArray();

        Stopwatch sw = Stopwatch.StartNew();

        if (synchronousTasks.Length > 0)
        {
            logger.LogInformation("Executing {numberOfTasks} synchronous tasks", synchronousTasks.Length);

            foreach (var startupTaskType in synchronousTasks)
            {
                await ExecuteTask(startupTaskType, logger);
            }
        }

        if (asynchronousTasks.Length > 0)
        {
            logger.LogInformation("Executing {numberOfTasks} asynchronous tasks. They will be executed in parallel.",
                                  asynchronousTasks.Length);

            await asynchronousTasks.ParallelForEachAsync(async (type) =>
            {
                await ExecuteTask(type, logger);
            });
        }

        sw.Stop();

        logger?.LogInformation("Start up tasks execution finished after {Elapsed}. " +
                               "Total {numberOfTasks} have executed.",
                               sw.Elapsed,
                               startupTaskTypes.Length);
    }

    private async Task ExecuteTask(Type    startupTaskType,
                                   ILogger logger)
    {
        var taskType = startupTaskType.Name;

        using (var backgroundTaskScope = serviceScopeFactory.CreateScope())
        {
            if (backgroundTaskScope.ServiceProvider
                                   .TryGet(startupTaskType) is not IStartupTask taskInstance)
            {
                logger.LogWarning("Cannot resolve task {taskType} from the service collection. Skipping...",
                                  taskType);

                return;
            }


            logger.LogInformation("Starting task {taskType} execution {syncState}...",
                                  taskType,
                                  taskInstance is ISynchronousStartupTask
                                      ? "synchronously"
                                      : "asynchronously");

            var sw = Stopwatch.StartNew();

            await taskInstance.Execute();

            logger.LogInformation("Finished task {taskType} execution after {Elapsed}.", taskType, sw.Elapsed);

            sw.Stop();
        }
    }
}