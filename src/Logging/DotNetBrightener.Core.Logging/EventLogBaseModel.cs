namespace DotNetBrightener.Core.Logging;

public abstract class EventLogBaseModel
{
    public string LoggerName { get; set; }

    public string Level { get; set; }

    public string FormattedMessage { get; set; }

    public string Message { get; set; }

    public DateTime TimeStamp { get; set; }

    public string RequestUrl { get; set; }

    public string RemoteIpAddress { get; set; }

    public string RequestId { get; set; }

    public string UserAgent { get; set; }

    public string TenantIds { get; set; }

    public string FullMessage { get; set; }

    public string StackTrace { get; set; }

    public string PropertiesDictionary { get; set; }
}