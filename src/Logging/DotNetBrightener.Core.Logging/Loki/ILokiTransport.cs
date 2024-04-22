using DotNetBrightener.Core.Logging.Loki.Model;

namespace DotNetBrightener.Core.Logging.Loki;

public interface ILokiTransport
{
    void WriteLogEvents(IEnumerable<LokiEvent> lokiEvents);

    Task WriteLogEventsAsync(IEnumerable<LokiEvent> lokiEvents);
}