using System;
using System.Collections.Generic;
using DotNetBrightener.Plugins.EventPubSub;
using Newtonsoft.Json;

namespace DotNetBrightener.Core.Logging;

public class EventLogEnqueueingEvent : IEventMessage
{
    public EventLogModel EventLogRecord { get; set; }
}

public abstract class EventLogBaseModel
{
    public string LoggerName { get; set; }
    public string Level { get; set; }
    public string FormattedMessage { get; set; }
    public string Message { get; set; }
    public DateTime TimeStamp { get; set; }
    public string RequestUrl { get; set; }
    public string RemoteIpAddress { get; set; }
    public string ClientId { get; set; }
    public string UserAgent { get; set; }
    public string TenantIds { get; set; }
}

public class EventLogModel : EventLogBaseModel
{
    public string FullMessage => Exception?.GetFullExceptionMessage();

    public string StackTrace => Exception?.StackTrace;

    [JsonIgnore]
    public IDictionary<object, object> Properties { get; set; }

    [JsonIgnore]
    public Exception Exception { get; set; }
}