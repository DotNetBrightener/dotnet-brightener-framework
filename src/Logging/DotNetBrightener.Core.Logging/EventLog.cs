using System.ComponentModel.DataAnnotations;
using DotNetBrightener.Core.Logging.Internals;

namespace DotNetBrightener.Core.Logging;

public class EventLog : EventLogModel
{
    [Key]
    public long Id { get; set; }

    public EventLog()
    {

    }

    public EventLog(EventLogBaseModel model)
    {
        this.UpdateFromDto(model);
    }
}