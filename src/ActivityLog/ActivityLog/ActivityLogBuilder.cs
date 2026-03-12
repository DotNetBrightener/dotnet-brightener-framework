using Microsoft.Extensions.DependencyInjection;

namespace ActivityLog;

/// <summary>
/// Builder for configuring Activity Logging services
/// </summary>
public class ActivityLogBuilder
{
    /// <summary>
    /// Gets the service collection
    /// </summary>
    public IServiceCollection Services { get; set; } = null!;
}
