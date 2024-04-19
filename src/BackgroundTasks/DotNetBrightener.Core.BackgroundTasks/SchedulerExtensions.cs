using System.Reflection;

namespace DotNetBrightener.Core.BackgroundTasks;

public static class SchedulerExtensions
{
    /// <summary>
    ///     Schedules a task that will be executed once.
    /// </summary>
    /// <param name="scheduler">The <see cref="IScheduler"/></param>
    /// <param name="methodAction">The method action to execute the task</param>
    /// <param name="parameters">The parameters to pass into <seealso cref="methodAction"/></param>
    public static void ScheduleTaskOnce(this IScheduler scheduler,
                                        MethodInfo      methodAction,
                                        params object[] parameters)
    {
        scheduler.ScheduleTask(methodAction, parameters)
                 .PreventOverlapping()
                 .Once();
    }

    /// <summary>
    ///     Schedules a task that will be executed once.
    /// </summary>
    /// <typeparam name="T">The type of task to schedule</typeparam>
    /// <param name="scheduler">The <see cref="IScheduler"/></param>
    public static void ScheduleTaskOnce<T>(this IScheduler scheduler)
        where T : IBackgroundTask
    {
        scheduler.ScheduleTask<T>()
                 .PreventOverlapping()
                 .Once();
    }

    /// <summary>
    ///     Schedules a task that will be executed once.
    /// </summary>
    /// <param name="scheduler">The <see cref="IScheduler"/></param>
    /// <param name="taskType">The type of task to schedule</param>
    public static void ScheduleTaskOnce(this IScheduler scheduler, Type taskType)
    {
        scheduler.ScheduleTask(taskType)
                 .PreventOverlapping()
                 .Once();
    }
}