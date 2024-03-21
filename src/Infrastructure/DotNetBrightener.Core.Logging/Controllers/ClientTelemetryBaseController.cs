using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DotNetBrightener.Core.Logging.Controllers;

[ApiController]
public abstract class ClientTelemetryBaseController : Controller
{
    protected readonly IEventLogDataService EventLogDataService;

    protected ClientTelemetryBaseController(IEventLogDataService eventLogDataService)
    {
        EventLogDataService = eventLogDataService;
    }

    [HttpPost("{loggerName}")]
    public virtual async Task<IActionResult> RecordLog(string loggerName, [FromBody] ClientTelemetryModel message)
    {
        var eventLogModel = new EventLog
        {
            LoggerName       = loggerName,
            Level            = message.Level.ToString(),
            FormattedMessage = message.FormattedMessage,
            Message          = message.Message,
            StackTrace       = message.StackTrace,
            TimeStamp        = DateTime.UtcNow,
            RequestUrl       = HttpContext.Request.GetRequestUrl(),
            RemoteIpAddress  = HttpContext.GetClientIP(),
            UserAgent        = HttpContext.Request.Headers.UserAgent
        };


        await EventLogDataService!.InsertAsync(eventLogModel);

        return NoContent();
    }
}

public class ClientTelemetryModel
{
    public LogLevel Level { get; set; } = LogLevel.Information;

    public string Message { get; set; }

    public string StackTrace { get; set; }

    public string FormattedMessage { get; set; }
}