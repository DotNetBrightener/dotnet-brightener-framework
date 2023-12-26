using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.BackgroundTasks;

public interface IBackgroundTaskRunner
{
    Task DoWork();
}

public class BackgroundTaskRunner : IBackgroundTaskRunner
{
    private readonly IEnumerable<IBackgroundTask> _tasks;
    private readonly ILogger                      _logger;
    private readonly IServiceScopeFactory         _serviceResolver;

    public BackgroundTaskRunner(IEnumerable<IBackgroundTask>  tasks,
                                ILogger<BackgroundTaskRunner> logger,
                                IServiceScopeFactory          serviceResolver)
    {
        _tasks           = tasks;
        _logger          = logger;
        _serviceResolver = serviceResolver;
    }

    public async Task DoWork()
    {
        if (!_tasks.Any())
        {
            return;
        }

        _logger.LogInformation($"Background tasks start executing");

        var tasksList = new List<Task>();

        foreach (var backgroundTask in _tasks)
        {
            tasksList.Add(ExecuteTask(backgroundTask.GetType()));
        }

        await Task.WhenAll(tasksList);
    }

    private async Task ExecuteTask(Type taskHandler)
    {
        using var scope = _serviceResolver.CreateScope();

        _logger.LogInformation($"Resolving task instance of type {taskHandler.FullName}...");
        var taskInstance = scope.ServiceProvider.GetService(taskHandler) as IBackgroundTask;

        if (taskInstance == null)
        {
            _logger.LogWarning($"Cannot resolve task instance of type {taskHandler.FullName}. Exiting...");
            return;
        }

        var taskName  = taskHandler.FullName;
        var stopWatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation($"Executing task ${taskName}...");
            await taskInstance.Execute();
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error while running background task {taskName}", exception);

            throw;
        }
        finally
        {
            stopWatch.Stop();
            _logger.LogInformation($"Finished executing task ${taskName} in {stopWatch.Elapsed}");
        }
    }
}