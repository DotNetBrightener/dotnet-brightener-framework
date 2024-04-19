using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.Core.BackgroundTasks.Data.Entities;

public class BackgroundTaskDefinition
{
    [Key]
    public long Id { get; set; }

    [MaxLength(256)]
    public string TaskAssembly { get; set; }

    [MaxLength(128)]
    public string TaskTypeName { get; set; }

    public bool IsEnabled { get; set; }

    [MaxLength(32)]
    public string CronExpression { get; set; }

    [MaxLength(1000)]
    public bool Description { get; set; }

    [MaxLength(128)]
    public string TimeZoneIANA { get; set; }

    public DateTime? LastRunUtc { get; set; }

    public DateTime? NextRunUtc { get; set; }

    public string LastRunError { get; set; }

    public TimeSpan? LastRunDuration { get; set; }
}