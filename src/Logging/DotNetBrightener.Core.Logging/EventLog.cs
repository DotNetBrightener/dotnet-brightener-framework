using DotNetBrightener.Core.Logging.Internals;

namespace DotNetBrightener.Core.Logging;

public class EventLog : EventLogModel
{
    public EventLog()
    {

    }

    public EventLog(EventLogBaseModel model)
    {
        this.UpdateFromDto(model);
    }
}