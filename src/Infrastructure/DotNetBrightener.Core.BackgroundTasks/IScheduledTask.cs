namespace DotNetBrightener.Core.BackgroundTasks;

/// <summary>
///     Represents a task that should be executed in background by the scheduler
/// </summary>
public interface IScheduledTask
{
    /// <summary>
    ///     Executes the task
    /// </summary>
    Task Execute();
}