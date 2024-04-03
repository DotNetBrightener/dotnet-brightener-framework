namespace DotNetBrightener.Core.BackgroundTasks;

public class TaskProcessResult
{
    public string TaskIdentifier { get; set; }

    public bool IsCompleted { get; set; }

    public DateTimeOffset? Started { get; set; }
}