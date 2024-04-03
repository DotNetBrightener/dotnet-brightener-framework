using DotNetBrightener.DataAccess.Utils;
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
        DataTransferObjectUtils.UpdateFromDto(this, model);
    }
}