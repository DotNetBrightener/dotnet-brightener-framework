namespace DotNetBrightener.Core.BackgroundTasks;

/// <summary>
///     Represents a task that should be executed in background throughout the lifetime of the application
/// </summary>
public interface IBackgroundTask
{
    /// <summary>
    ///     Executes the task
    /// </summary>
    Task Execute();
}