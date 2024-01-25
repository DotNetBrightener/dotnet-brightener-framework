using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Threading;
using LokiTarget = DotNetBrightener.Core.Logging.Loki.LokiTarget;
using LokiTargetLabel = DotNetBrightener.Core.Logging.Loki.LokiTargetLabel;

namespace DotNetBrightener.Core.Logging;

[Target("EventLoggingWatcher")]
public class EventLoggingWatcher : TargetWithLayout, IEventLogWatcher
{
    private readonly        IHostEnvironment _webHostEnvironment;
    public static           EventLoggingWatcher Instance { get; private set; }
    private static readonly Layout RequestUrlLayoutRenderer = Layout.FromString("${aspnet-request-url}");
    private static readonly Layout RequestIpLayoutRenderer = Layout.FromString("${aspnet-request-ip}");
    private static readonly Layout RequestUserAgentLayoutRenderer = Layout.FromString("${aspnet-request-useragent}");

    private const string DefaultLogLayout =
        "[${longdate}] | ${aspnet-traceidentifier} | ${event-properties:item=EventId.Id} | [${logger}] | ${uppercase:${level}} | ${message} ${exception:format=ToString,StackTrace} | requesturl=${aspnet-request-url} | requestip=${aspnet-request-ip:CheckForwardedForHeader=true} | useragent=${aspnet-request-useragent}";

    private readonly FileTarget           _fileTarget;
    private readonly ConsoleTarget        _consoleTarget;
    private          LokiTarget           _lokiTarget;
    private readonly Queue<EventLogModel> _queue = new();
    private          IServiceScopeFactory _serviceScopeFactory;
    private          bool                 _serviceProviderSet;

    public static void Initialize(IHostEnvironment environment)
    {
        Instance = new EventLoggingWatcher(environment);
    }

    public EventLoggingWatcher(IHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
        _fileTarget = new("fileTarget")
        {
            ArchiveFileName   = Layout.FromString("${var:configDir}/logs/Archive/{#}/log-${level}.log"),
            FileName          = Layout.FromString("${var:configDir}/logs/${shortdate}/log-${level}.log"),
            ArchiveDateFormat = "yyyy-MM-dd",
            ArchiveEvery      = FileArchivePeriod.Day,
            ArchiveNumbering  = ArchiveNumberingMode.Date,
            MaxArchiveFiles   = 30,
            Layout            = Layout.FromString(DefaultLogLayout)
        };

        _consoleTarget = new ConsoleTarget();
    }

    internal void SetServiceScopeFactory(IServiceScopeFactory serviceScopeFactory)
    {
        if (_serviceProviderSet)
            throw new InvalidOperationException("Service Provider can be configured once");

        _serviceScopeFactory = serviceScopeFactory;
        _serviceProviderSet  = true;
    }

    internal void SetLokiTarget(string lokiEndpoint,
                                string applicationName = null,
                                string userName        = null,
                                string password        = null)
    {
        _lokiTarget = new LokiTarget
        {
            Endpoint = Layout.FromString(lokiEndpoint),
            Layout   = Layout.FromString(DefaultLogLayout),
            UserName = userName,
            Password = password,
            Labels =
            {
                new LokiTargetLabel
                {
                    Name   = "Application",
                    Layout = Layout.FromString(applicationName ?? _webHostEnvironment.ApplicationName)
                },
                new LokiTargetLabel
                {
                    Name   = "Environment",
                    Layout = Layout.FromString(_webHostEnvironment.EnvironmentName)
                }
            }
        };
    }

    public List<EventLogModel> GetQueuedEventLogRecords()
    {
        lock (_queue)
        {
            if (_queue.Count == 0)
                return new List<EventLogModel>();

            List<EventLogModel> queuedEventLogRecords = [.._queue];
            _queue.Clear();

            return queuedEventLogRecords;
        }
    }

    protected override void Write(LogEventInfo logEvent)
    {
        var asyncLogEventInfo = new AsyncLogEventInfo(logEvent, Continuation);

        _fileTarget.WriteAsyncLogEvent(asyncLogEventInfo);
        _consoleTarget.WriteAsyncLogEvent(asyncLogEventInfo);
        _lokiTarget?.WriteAsyncTask(logEvent, CancellationToken.None);

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

        _queue.Enqueue(eventLogModel);

        if (_serviceScopeFactory == null) // when app not fully initialized, don't put logs to queue
            return;

        using var scope = _serviceScopeFactory.CreateScope();

        var eventPublisher = scope.ServiceProvider
                                  .GetService<IEventPublisher>();

        if (string.IsNullOrEmpty(eventLogModel.RequestUrl))
        {
            var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();

            if (httpContextAccessor != null &&
                httpContextAccessor.HttpContext != null)
            {
                eventLogModel.RequestUrl      = httpContextAccessor.HttpContext.Request.GetRequestUrl();
                eventLogModel.RemoteIpAddress = httpContextAccessor.HttpContext.GetClientIP();
                eventLogModel.UserAgent       = httpContextAccessor.HttpContext.Request.Headers.UserAgent;
            }
        }

        eventPublisher?.Publish(new EventLogEnqueueingEvent
                        {
                            EventLogRecord = eventLogModel
                        })
                       .Wait();
    }

    private void Continuation(Exception exception)
    {
        if (exception != null)
        {
            Console.WriteLine(exception.ToString());
        }
    }
}