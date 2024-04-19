namespace DotNetBrightener.Core.BackgroundTasks;

public interface ICancellableTask : IBackgroundTask
{
    CancellationToken CancellationToken { get; set; }
}