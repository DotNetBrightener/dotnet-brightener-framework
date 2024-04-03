using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.BackgroundTasks;

public interface IBackgroundTaskRunner
{
    Task DoWork();
}

public class BackgroundTaskRunner : IBackgroundTaskRunner
{
    private readonly Type[]               _tasks;
    private readonly ILogger              _logger;
    private readonly IServiceScopeFactory _serviceResolver;

    public BackgroundTaskRunner(IEnumerable<IBackgroundTask>  tasks,
                                ILogger<BackgroundTaskRunner> logger,
                                IServiceScopeFactory          serviceResolver)
    {
        _logger          = logger;
        _serviceResolver = serviceResolver;

        _tasks = tasks.Select(task => task.GetType())
                      .ToArray();
    }

    public async Task DoWork()
    {
        if (!_tasks.Any())
        {
            return;
        }

        _logger.LogInformation("Background tasks start executing");

        var stopWatch = Stopwatch.StartNew();

        await Parallel.ForEachAsync(_tasks,
                                    async (type, ct) => await ExecuteTask(type));

        stopWatch.Stop();
        _logger.LogInformation("Finished executing {numberOfTasks} background tasks in {elapsed}.",
                               _tasks.Length,
                               stopWatch.Elapsed);
    }

    private async Task ExecuteTask(Type taskHandler)
    {
        using var scope = _serviceResolver.CreateScope();

        var taskName = taskHandler.FullName;

        _logger.LogInformation("Resolving task instance of type {taskName}...", taskName);

        if (scope.ServiceProvider.GetService(taskHandler) is not IBackgroundTask taskInstance)
        {
            _logger.LogWarning("Cannot resolve task instance of type {taskName}. Exiting...", taskName);

            return;
        }

        var stopWatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Executing task {taskName}...", taskName);
            await taskInstance.Execute();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error while running background task {taskName}", taskName);

            throw;
        }
        finally
        {
            stopWatch.Stop();
            _logger.LogInformation("Finished executing task {taskName} in {elapsed}", taskName, stopWatch.Elapsed);
        }
    }
}