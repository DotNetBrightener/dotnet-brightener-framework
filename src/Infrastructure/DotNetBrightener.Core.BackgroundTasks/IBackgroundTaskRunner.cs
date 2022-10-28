using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

    public BackgroundTaskRunner(IEnumerable<IBackgroundTask>  tasks,
                                ILogger<BackgroundTaskRunner> logger)
    {
        _tasks  = tasks;
        _logger = logger;
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
            tasksList.Add(ExecuteTask(backgroundTask));
        }

        await Task.WhenAll(tasksList);
    }

    private async Task ExecuteTask(IBackgroundTask taskHandler)
    {
        var taskName  = taskHandler.GetType().FullName;
        var stopWatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation($"Executing task ${taskName}");
            await taskHandler.Execute();
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