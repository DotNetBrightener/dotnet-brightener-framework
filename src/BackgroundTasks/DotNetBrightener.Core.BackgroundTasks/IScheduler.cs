using System.Reflection;

namespace DotNetBrightener.Core.BackgroundTasks;

public interface IScheduler
{
    Task RunAt(DateTime tick);

    /// <summary>
    ///     Schedules a task from given action method using provided parameters
    /// </summary>
    /// <param name="methodAction">
    ///     The <see cref="MethodInfo"/> of the task to be executed
    /// </param>
    /// <param name="parameters">
    ///     The parameters to be passed to the <see cref="methodAction"/> when executes the task
    /// </param>
    /// <returns>
    ///     The <seealso cref="IScheduleInterval"/> for configuring intervals
    /// </returns>
    IScheduleConfig ScheduleTask(MethodInfo methodAction, params object[] parameters);

    /// <summary>
    ///     Schedules a task from given task type
    /// </summary>
    /// <typeparam name="T">The type of task to execute</typeparam>
    /// <returns>
    ///     The <seealso cref="IScheduleInterval"/> for configuring intervals
    /// </returns>
    IScheduleConfig ScheduleTask<T>() where T : IBackgroundTask;

    /// <summary>
    ///     Schedules a task from given task type
    /// </summary>
    /// <param name="taskType">
    ///     The type of task to execute
    /// </param>
    /// <returns>
    ///     The <seealso cref="IScheduleInterval"/> for configuring intervals
    /// </returns>
    IScheduleConfig ScheduleTask(Type taskType);

    /// <summary>
    ///     Tries to unschedule a task by its unique identifier
    /// </summary>
    /// <param name="uniqueIdentifier">The unique identifier of the task</param>
    /// <returns></returns>
    bool TryUnscheduleTask(string uniqueIdentifier);

    /// <summary>
    ///     Attempts to cancel all the tasks that can be cancelled
    /// </summary>
    /// <returns></returns>
    Task CancelAllCancellableTasks();

    /// <summary>
    ///     Indicates if the scheduler is executing tasks
    /// </summary>
    bool IsRunning { get; }
}