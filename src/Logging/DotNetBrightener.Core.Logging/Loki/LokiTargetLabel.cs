using NLog.Config;
using NLog.Layouts;

namespace DotNetBrightener.Core.Logging.Loki;

[NLogConfigurationItem]
public class LokiTargetLabel
{
    [RequiredParameter]
    public string Name { get; set; }

    [RequiredParameter]
    public Layout Layout { get; set; }
}