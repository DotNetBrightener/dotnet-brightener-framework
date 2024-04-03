namespace DotNetBrightener.Core.BackgroundTasks;

public class QueuedTaskResult
{
    public string TaskIdentifier { get; set; }

    public Task<dynamic> TaskResult { get; set; }

    public DateTimeOffset? Started { get; set; }
}