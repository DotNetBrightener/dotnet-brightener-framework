using DotNetBrightener.Plugins.EventPubSub;

namespace DotNetBrightener.Core.BackgroundTasks.Event;

public abstract class ScheduledEventMessage : IEventMessage
{
    public readonly ScheduledTask ScheduledTask;

    protected ScheduledEventMessage(ScheduledTask scheduledTask)
    {
        ScheduledTask = scheduledTask;
    }
}

public class ScheduledEventStarted : ScheduledEventMessage
{
    public ScheduledEventStarted(ScheduledTask scheduledTask): base(scheduledTask)
    {
    }
}

public class ScheduledEventEnded : ScheduledEventMessage
{
    public ScheduledEventEnded(ScheduledTask scheduledTask): base(scheduledTask)
    {
    }
}

public class ScheduledEventFailed : ScheduledEventMessage
{
    public readonly Exception Exception;

    public ScheduledEventFailed(ScheduledTask scheduledTask, Exception exception): base(scheduledTask)
    {
        Exception = exception;
    }
}