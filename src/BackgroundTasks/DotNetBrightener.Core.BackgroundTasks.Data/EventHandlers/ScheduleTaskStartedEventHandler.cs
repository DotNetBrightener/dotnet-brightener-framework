using DotNetBrightener.Core.BackgroundTasks.Event;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.BackgroundTasks.Data.EventHandlers;

public class ScheduleTaskStartedEventHandler : IEventHandler<ScheduledEventStarted>
{
    private readonly ILogger _logger;

    public ScheduleTaskStartedEventHandler(ILogger<ScheduleTaskStartedEventHandler> logger)
    {
        _logger = logger;
    }

    public int Priority => 1000;

    public async Task<bool> HandleEvent(ScheduledEventStarted eventMessage)
    {

        return true;
    }
}