using System.Text;
using DotNetBrightener.Core.Logging.Loki.Model;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace DotNetBrightener.Core.Logging.Loki;

[Target("loki")]
public class LokiTarget : TargetWithLayout
{
    private readonly Lazy<ILokiTransport> lazyLokiTransport;

    [RequiredParameter]
    public Layout Endpoint { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }

    [ArrayParameter(typeof(LokiTargetLabel), "label")]
    public IList<LokiTargetLabel> Labels { get; }

    public LokiTarget()
    {
        Labels = new List<LokiTargetLabel>();

        lazyLokiTransport =
            new Lazy<ILokiTransport>(
                                     () => GetLokiTransport(Endpoint),
                                     LazyThreadSafetyMode.ExecutionAndPublication);
    }

    protected override void Write(IList<AsyncLogEventInfo> logEvents)
    {
        var events = GetLokiEvents(logEvents.Select(alei => alei.LogEvent));
        lazyLokiTransport.Value.WriteLogEvents(events);
    }

    public Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
    {
        var @event = GetLokiEvent(logEvent);

        return lazyLokiTransport.Value.WriteLogEventsAsync([
            @event
        ]);
    }

    public Task WriteAsyncTask(IList<LogEventInfo> logEvents, CancellationToken cancellationToken)
    {
        var events = GetLokiEvents(logEvents);

        return lazyLokiTransport.Value.WriteLogEventsAsync(events);
    }

    private IEnumerable<LokiEvent> GetLokiEvents(IEnumerable<LogEventInfo> logEvents)
    {
        return logEvents.Select(GetLokiEvent);
    }

    private LokiEvent GetLokiEvent(LogEventInfo logEvent)
    {
        var labels =
            new LokiLabels(
                           Labels.Select(
                                         ltl => new LokiLabel(ltl.Name, ltl.Layout.Render(logEvent))));

        var line = RenderLogEvent(Layout, logEvent);

        var @event = new LokiEvent(labels, logEvent.TimeStamp, line);

        return @event;
    }

    internal ILokiTransport GetLokiTransport(Layout endpoint)
    {
        var endpointUri = RenderLogEvent(endpoint, LogEventInfo.CreateNullEvent());

        if (Uri.TryCreate(endpointUri, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme == Uri.UriSchemeHttp ||
                uri.Scheme == Uri.UriSchemeHttps)
            {
                var lokiHttpClient    = GetLokiHttpClient(uri, UserName, Password);
                var httpLokiTransport = new HttpLokiTransport(uri, lokiHttpClient);

                return httpLokiTransport;
            }
        }

        InternalLogger.Warn("Unable to create a valid Loki Endpoint URI from '{0}'", endpoint);

        var nullLokiTransport = new NullLokiTransport();

        return nullLokiTransport;
    }

    internal static ILokiHttpClient GetLokiHttpClient(Uri uri, string username, string password)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = uri
        };
        var lokiHttpClient = new LokiHttpClient(httpClient);


        if (!string.IsNullOrEmpty(username) &&
            !string.IsNullOrEmpty(password))
        {
            var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        return lokiHttpClient;
    }
}