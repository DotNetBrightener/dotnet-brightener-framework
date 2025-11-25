using DotNetBrightener.Core.Logging.Internals;
using System.ComponentModel.DataAnnotations;

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