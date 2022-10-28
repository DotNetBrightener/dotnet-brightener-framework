using System;
using System.Collections.Generic;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace DotNetBrightener.Core.Logging;

[Target("EventLoggingWatcher")]
public class EventLoggingWatcher : TargetWithLayout, IEventLogWatcher
{
    public static EventLoggingWatcher Instance { get; } = new();
    private static readonly Layout RequestUrlLayoutRenderer = Layout.FromString("${aspnet-request-url}");
    private static readonly Layout RequestIpLayoutRenderer = Layout.FromString("${aspnet-request-ip}");
    private static readonly Layout RequestUserAgentLayoutRenderer = Layout.FromString("${aspnet-request-useragent}");

    private const string DefaultLogLayout =
        "[${longdate}] | ${aspnet-traceidentifier} | ${event-properties:item=EventId.Id} | [${logger}] | ${uppercase:${level}} | ${message} ${exception:format=ToString,StackTrace}";

    private readonly FileTarget           _fileTarget;
    private readonly Queue<EventLogModel> _queue = new();
    private          IServiceProvider     _serviceProvider;
    private          bool                 _serviceProviderSet;

    public EventLoggingWatcher()
    {
        _fileTarget = new("fileTarget")
        {
            ArchiveFileName = Layout.FromString("${var:configDir}/logs/Archive/{#}/log-${level}.log"),
            FileName = Layout.FromString("${var:configDir}/logs/${shortdate}/log-${level}.log"),
            ArchiveDateFormat = "yyyy-MM-dd",
            ArchiveEvery = FileArchivePeriod.Day,
            ArchiveNumbering = ArchiveNumberingMode.Date,
            MaxArchiveFiles = 30,
            Layout = Layout.FromString(DefaultLogLayout)
        };
    }
    
    internal void SetServiceProvider(IServiceProvider serviceProvider)
    {
        if (_serviceProviderSet)
            throw new InvalidOperationException("Service Provider can be configured once");

        _serviceProvider    = serviceProvider;
        _serviceProviderSet = true;
    }

    public List<EventLogModel> GetQueuedEventLogRecords()
    {
        List<EventLogModel> queuedEventLogRecords = new(_queue);
        _queue.Clear();
        return queuedEventLogRecords;
    }

    protected override void Write(LogEventInfo logEvent)
    {
        _fileTarget.WriteAsyncLogEvent(new AsyncLogEventInfo(logEvent, Continuation));

        if (_serviceProvider == null) // when app not fully initialized, don't put logs to queue
            return;

        var eventLogModel = new EventLogModel
        {
            Exception        = logEvent.Exception,
            FormattedMessage = logEvent.FormattedMessage,
            Level            = logEvent.Level.ToString(),
            LoggerName       = logEvent.LoggerName,
            Properties       = logEvent.Properties,
            Message          = logEvent.Message,
            TimeStamp        = logEvent.TimeStamp.ToUniversalTime(),
            RequestUrl       = RequestUrlLayoutRenderer.Render(logEvent),
            RemoteIpAddress  = RequestIpLayoutRenderer.Render(logEvent),
            UserAgent        = RequestUserAgentLayoutRenderer.Render(logEvent)
        };

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventPublisher = scope.ServiceProvider.GetService<IEventPublisher>();
            eventPublisher!.Publish(new EventLogEnqueueingEvent
                            {
                                EventLogRecord = eventLogModel
                            })
                           .Wait();
        }

        _queue.Enqueue(eventLogModel);
    }

    private void Continuation(Exception exception)
    {
        if (exception != null)
        {
            // do something
        }
    }
}