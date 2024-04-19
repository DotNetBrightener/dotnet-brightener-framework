namespace DotNetBrightener.Core.BackgroundTasks.Options;

public class BackgroundTaskOptions
{
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);
}
