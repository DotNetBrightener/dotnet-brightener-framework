using System.ComponentModel.DataAnnotations;
using DotNetBrightener.DataTransferObjectUtility;

namespace DotNetBrightener.Core.Logging;

public class EventLog : EventLogBaseModel
{
    [Key]
    public long Id { get; set; }

    public string FullMessage { get; set; }

    public string StackTrace { get; set; }

    public EventLog()
    {

    }

    public EventLog(EventLogModel model)
    {
        DataTransferObjectUtils.UpdateFromDto(this, model);
    }
}