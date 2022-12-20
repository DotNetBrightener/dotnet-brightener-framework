using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetBrightener.Core.Logging.Loki.Model;

namespace DotNetBrightener.Core.Logging.Loki
{
    public class NullLokiTransport : ILokiTransport
    {
        public void WriteLogEvents(IEnumerable<LokiEvent> lokiEvents)
        {
        }

        public Task WriteLogEventsAsync(IEnumerable<LokiEvent> lokiEvents)
        {
            return Task.CompletedTask;
        }
    }
}
