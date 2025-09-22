using Microsoft.AspNetCore.Mvc;

namespace WebApp.CommonShared.AsyncTasks;

/// <summary>
///     Provide the base API controller for scheduling an asynchronous task
/// </summary>
[ApiController]
public abstract class AsyncTaskApiControllerBase(
    IAsyncTaskScheduler scheduler,
    IAsyncTaskContainer taskContainer) : Controller
{
    [NonAction]
    protected virtual async Task<Guid> ScheduleTask<TTaskExecutor, TInput>(TInput input)
        where TInput : class
        where TTaskExecutor : class, IAsyncTask<TInput>
    {
        var taskContext = new AsyncTaskContext
        {
            Input    = input,
            TaskName = typeof(TTaskExecutor).Name
        };

        var taskId = await scheduler.ScheduleTask<TTaskExecutor, TInput>(taskContext);

        return taskId;
    }

    /// <summary>
    ///     Retrieve status of the asynchronous task
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    [HttpGet("async/{taskId}/status")]
    public virtual async Task<IActionResult> GetTaskStatus(Guid taskId)
    {
        var task = await taskContainer.GetTask(taskId);

        if (task is null)
            return NotFound();

        return Ok(task);
    }

    /// <summary>
    ///     Retrieve the result of the asynchronous task.
    ///     The result could be error.
    /// </summary>
    /// <param name="taskId"></param>
    /// <returns></returns>
    [HttpGet("async/{taskId}/result")]
    public virtual async Task<IActionResult> GetTaskResult(Guid taskId)
    {
        var task = await taskContainer.GetTask(taskId);

        return Ok(task?.Result);
    }
}
