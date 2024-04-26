namespace DotNetBrightener.Core.Logging.Options;

public class LoggingRetentions
{
    public int ErrorRetentionsInDay { get; set; } = 60;

    public int WarningRetentionsInDay { get; set; } = 30;
    
    public int DefaultRetentionsInDay { get; set; } = 7;

    public Dictionary<string, TimeSpan> LoggerRules { get; set; } = new();
}
