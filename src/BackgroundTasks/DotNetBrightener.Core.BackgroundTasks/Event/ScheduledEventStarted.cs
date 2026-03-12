using DotNetBrightener.Plugins.EventPubSub;

namespace DotNetBrightener.Core.BackgroundTasks.Event;

public abstract class ScheduledEventMessage(ScheduledTask scheduledTask) : IEventMessage
{
    public readonly ScheduledTask ScheduledTask = scheduledTask;
}

public class ScheduledEventStarted(ScheduledTask scheduledTask) : ScheduledEventMessage(scheduledTask);

public class ScheduledEventEnded(ScheduledTask scheduledTask) : ScheduledEventMessage(scheduledTask);

public class ScheduledEventFailed(ScheduledTask scheduledTask, Exception exception)
    : ScheduledEventMessage(scheduledTask)
{
    public readonly Exception Exception = exception;
}