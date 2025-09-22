using System.Collections.Concurrent;

namespace WebApp.CommonShared.AsyncTasks;

public interface IAsyncTaskContainer : ISingletonDependency
{
    Task<AsyncTaskContext> GetTask(Guid taskId);

    Task<Guid> ScheduleTask(AsyncTaskContext taskContext);
}

public class AsyncTaskContainer : IAsyncTaskContainer
{
    private readonly ConcurrentDictionary<Guid, AsyncTaskContext> _tasksList = new();

    public async Task<AsyncTaskContext> GetTask(Guid taskId)
    {
        if (_tasksList.TryGetValue(taskId, out var taskContext))
        {
            return taskContext;
        }

        return default;
    }

    public async Task<Guid> ScheduleTask(AsyncTaskContext taskContext)
    {
        _tasksList.TryAdd(taskContext.TaskId, taskContext);

        return taskContext.TaskId;
    }
}