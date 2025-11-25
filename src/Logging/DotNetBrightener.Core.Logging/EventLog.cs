using DotNetBrightener.Core.Logging.Internals;
using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.Core.Logging;

public class EventLog : EventLogModel
{
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public EventLog()
    {

    }

    public EventLog(EventLogBaseModel model)
    {
        this.UpdateFromDto(model);
    }
}