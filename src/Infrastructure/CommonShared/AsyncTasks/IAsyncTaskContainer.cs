using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace WebApp.CommonShared.AsyncTasks;

public interface IAsyncTaskContainer : ISingletonDependency, IDisposable
{
    Task<AsyncTaskContext> GetTask(Guid taskId);

    Task<Guid> ScheduleTask(AsyncTaskContext taskContext);
}

public class AsyncTaskContainer : IAsyncTaskContainer
{
    private readonly ConcurrentDictionary<Guid, AsyncTaskContext> _tasksList = new();
    private readonly Timer                                        _cleanupTimer;
    private readonly ILogger<AsyncTaskContainer>                  _logger;

    public AsyncTaskContainer(ILogger<AsyncTaskContainer> logger)
    {
        _logger       = logger;
        _cleanupTimer = new Timer(DoCleanup, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public async Task<AsyncTaskContext> GetTask(Guid taskId)
    {
        if (_tasksList.TryGetValue(taskId, out var taskContext))
        {
            return taskContext;
        }

        return null;
    }

    public async Task<Guid> ScheduleTask(AsyncTaskContext taskContext)
    {
        _tasksList.TryAdd(taskContext.TaskId, taskContext);

        return taskContext.TaskId;
    }


    private void DoCleanup(object? state)
    {
        _logger.LogDebug("Starting async task cleanup process");

        CleanupExpiredTasks().Wait();
    }

    internal async Task CleanupExpiredTasks()
    {
        var now           = DateTimeOffset.UtcNow;
        var tasksToRemove = new List<Guid>();

        foreach (var kvp in _tasksList)
        {
            var taskContext = kvp.Value;

            // Check if task result was retrieved and retention period has elapsed
            if (taskContext.ResultRetrieved.HasValue)
            {
                var timeSinceRetrieved = now - taskContext.ResultRetrieved.Value;

                if (timeSinceRetrieved >= TimeSpan.FromMinutes(5))
                {
                    tasksToRemove.Add(kvp.Key);

                    continue;
                }
            }

            // Check if task is completed and retention period has elapsed
            if (taskContext.CompletedAt.HasValue)
            {
                var timeSinceCompleted = now - taskContext.CompletedAt.Value;

                if (timeSinceCompleted >= TimeSpan.FromMinutes(60))
                {
                    tasksToRemove.Add(kvp.Key);
                }
            }
        }

        // Remove the expired tasks
        foreach (var taskId in tasksToRemove)
        {
            _tasksList.TryRemove(taskId, out _);
        }
    }

    public void Dispose()
    {
        _cleanupTimer.Dispose();
    }
}