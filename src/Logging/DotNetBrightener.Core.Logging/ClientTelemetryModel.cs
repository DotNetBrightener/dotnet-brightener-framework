using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.Logging;

public class ClientTelemetryModel
{
    public LogLevel Level { get; set; } = LogLevel.Information;

    public string Message { get; set; }

    public string StackTrace { get; set; }

    public string[] Properties { get; set; }
}

