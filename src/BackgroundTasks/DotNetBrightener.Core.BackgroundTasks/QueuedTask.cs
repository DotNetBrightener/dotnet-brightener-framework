using DotNetBrightener.Core.BackgroundTasks.Cron;
using System.Reflection;

namespace DotNetBrightener.Core.BackgroundTasks;

/// <summary>
///     Represents a task that is queued to be executed in the background
/// </summary>
public class QueuedTask
{
    public string TaskIdentifier { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    ///     The method that needs to be executed in background
    /// </summary>
    public MethodInfo Action { get; set; }

    /// <summary>
    ///     The parameters to send to the action
    /// </summary>
    public object[] Parameters { get; set; }

    public DateTimeOffset? StartedOn { get; set; }

    internal Task<dynamic> TaskResult { get; set; }

    public string TaskName => $"{Action.DeclaringType?.FullName}.{Action.Name}()";

    private CronExpression _expression;
}