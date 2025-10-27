using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using LokiTarget = DotNetBrightener.Core.Logging.Loki.LokiTarget;
using LokiTargetLabel = DotNetBrightener.Core.Logging.Loki.LokiTargetLabel;

namespace DotNetBrightener.Core.Logging;

[Target("EventLoggingWatcher")]
public class EventLoggingWatcher : TargetWithLayout, IEventLogWatcher
{
    private readonly IHostEnvironment _webHostEnvironment;

    public static           EventLoggingWatcher Instance { get; private set; }
    private static readonly Layout              RequestUrlLayoutRenderer       = Layout.FromString("${aspnet-request-url}");
    private static readonly Layout              RequestIpLayoutRenderer        = Layout.FromString("${aspnet-request-ip}");
    private static readonly Layout              RequestUserAgentLayoutRenderer = Layout.FromString("${aspnet-request-useragent}");

    private static readonly Layout TraceIdentifierLayoutRenderer =
        Layout.FromString("${aspnet-TraceIdentifier:ignoreActivityId=boolean}");

    private const string DefaultLogLayout =
        "[${longdate}] | ${aspnet-traceidentifier} | ${event-properties:item=EventId.Id} | [${logger}] | ${uppercase:${level}} | ${message} ${exception:format=ToString,StackTrace} | requesturl=${aspnet-request-url} | requestip=${aspnet-request-ip:CheckForwardedForHeader=true} | useragent=${aspnet-request-useragent}";

    private                 LokiTarget               _lokiTarget;
    private readonly        Queue<EventLogBaseModel> _queue = new();
    private                 IServiceScopeFactory     _serviceScopeFactory;
    private                 bool                     _serviceProviderSet;
    private static readonly Lock                     Lock = new();

    public static void Initialize(IHostEnvironment environment, IConfiguration configuration)
    {
        Instance = new EventLoggingWatcher(environment, configuration);
    }

    private EventLoggingWatcher(IHostEnvironment webHostEnvironment, IConfiguration configuration)
    {
        _webHostEnvironment = webHostEnvironment;
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

    public List<EventLogBaseModel> GetQueuedEventLogRecords()
    {
        lock (Lock)
        {
            if (_queue.Count == 0)
                return [];

            List<EventLogBaseModel> queuedEventLogRecords = [];

            while (_queue.TryDequeue(out var eventLogModel))
            {
                queuedEventLogRecords.Add(eventLogModel);
            }

            _queue.Clear();

            return queuedEventLogRecords;
        }
    }

    protected override void Write(LogEventInfo logEvent)
    {
        // check if log level is enabled before writing
        if (!ShouldWrite(logEvent, LoggingConfiguration))
            return;

        _lokiTarget?.WriteAsyncTask(logEvent, CancellationToken.None);

        EventLogBaseModel eventLogModel =
            logEvent.Exception is InformativeStackTrace stackTraceOnlyException
                ? new ClientTelemetryLogModel()
                {
                    StackTrace = stackTraceOnlyException.Message
                }
                : new EventLogModel();


        eventLogModel.FormattedMessage = logEvent.FormattedMessage;
        eventLogModel.Level            = logEvent.Level.ToString();
        eventLogModel.LoggerName       = logEvent.LoggerName;
        eventLogModel.Message          = logEvent.Message;
        eventLogModel.TimeStamp        = logEvent.TimeStamp.ToUniversalTime();
        eventLogModel.RequestUrl       = RequestUrlLayoutRenderer.Render(logEvent);
        eventLogModel.RemoteIpAddress  = RequestIpLayoutRenderer.Render(logEvent);
        eventLogModel.UserAgent        = RequestUserAgentLayoutRenderer.Render(logEvent);
        eventLogModel.RequestId        = TraceIdentifierLayoutRenderer.Render(logEvent);

        if (logEvent.Properties is not null)
        {
            try
            {
                eventLogModel.PropertiesDictionary = JsonConvert.SerializeObject(logEvent.Properties,
                                                                                 settings: new JsonSerializerSettings
                                                                                 {
                                                                                     ReferenceLoopHandling =
                                                                                         ReferenceLoopHandling.Ignore
                                                                                 });
            }
            catch (Exception ex)
            {
                InternalLogger.Error(ex, "Error serializing properties");
            }
        }

        if (logEvent.Exception != null &&
            logEvent.Exception is not InformativeStackTrace)
        {
            eventLogModel.FullMessage = logEvent.Exception.GetFullExceptionMessage();
            eventLogModel.StackTrace  = logEvent.Exception.StackTrace;
        }

        if (logEvent.Exception is InformativeStackTrace)
        {
            logEvent.Exception = null;
        }

        lock (Lock)
        {
            _queue.Enqueue(eventLogModel);
        }

        if (_serviceScopeFactory == null) // when app not fully initialized, don't put logs to queue
            return;

        if (string.IsNullOrEmpty(eventLogModel.RequestUrl))
        {
            using var scope               = _serviceScopeFactory.CreateScope();
            var       httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();

            if (httpContextAccessor is { HttpContext: not null })
            {
                eventLogModel.RequestUrl      = httpContextAccessor.HttpContext.Request.GetRequestUrl();
                eventLogModel.RemoteIpAddress = httpContextAccessor.HttpContext.GetClientIP();
                eventLogModel.UserAgent       = httpContextAccessor.HttpContext.Request.Headers.UserAgent;
                eventLogModel.RequestId       = httpContextAccessor.HttpContext.TraceIdentifier;
            }
        }
    }

    private static bool ShouldWrite(LogEventInfo logEvent, LoggingConfiguration loggingConfiguration)
    {
        var matchingRule = loggingConfiguration.LoggingRules.FirstOrDefault(r => r.NameMatches(logEvent.LoggerName));

        if (matchingRule is { Final: true })
        {
            var shouldWrite = matchingRule.Levels.Count == 0 || matchingRule.Levels.Contains(logEvent.Level);

            return shouldWrite;
        }

        var defaultRule = loggingConfiguration.LoggingRules.FirstOrDefault(r => r.LoggerNamePattern == "*");

        if (defaultRule != null)
        {
            var shouldWrite = defaultRule.Levels.Count == 0 || defaultRule.Levels.Contains(logEvent.Level);

            return shouldWrite;
        }

        return false;
    }
}
