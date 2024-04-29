namespace DotNetBrightener.Core.Logging.Options;

public class LoggingRetentions
{
    public int ErrorRetentionsInDay { get; set; } = 60;

    public int WarningRetentionsInDay { get; set; } = 30;

    public TimeSpan DefaultRetentions { get; set; } = TimeSpan.FromDays(7);

    public Dictionary<string, TimeSpan> LoggerRules { get; set; } = new();
}