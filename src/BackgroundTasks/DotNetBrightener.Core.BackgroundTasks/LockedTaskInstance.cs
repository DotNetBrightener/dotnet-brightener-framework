namespace DotNetBrightener.Core.BackgroundTasks;

internal class LockedTaskInstance
{
    public bool IsLocked { get; set; }

    public DateTime? ExpiresAt { get; set; }
}