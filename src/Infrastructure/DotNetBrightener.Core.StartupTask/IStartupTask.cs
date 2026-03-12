namespace DotNetBrightener.Core.StartupTask;

/// <summary>
///     Represents a task that is executed at the application startup, can be asynchronous
/// </summary>
public interface IStartupTask
{
    /// <summary>
    ///     The order of execution. Lower number is executed first
    /// </summary>
    int Order { get; }

    /// <summary>
    ///     Executes the task
    /// </summary>
    /// <returns></returns>
    Task Execute();
}

/// <summary>
///    Represents a task that is executed at the application startup synchronously
/// </summary>
public interface ISynchronousStartupTask : IStartupTask;