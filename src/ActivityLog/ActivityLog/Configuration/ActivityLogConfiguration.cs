namespace ActivityLog.Configuration;

/// <summary>
/// Configuration options for the Activity Logging system
/// </summary>
public class ActivityLogConfiguration
{
    /// <summary>
    /// Gets or sets whether activity logging is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum log level for activities
    /// </summary>
    public ActivityLogLevel MinimumLogLevel { get; set; } = ActivityLogLevel.Information;

    /// <summary>
    /// Gets or sets the serialization configuration
    /// </summary>
    public SerializationConfiguration Serialization { get; set; } = new();

    /// <summary>
    /// Gets or sets the performance configuration
    /// </summary>
    public PerformanceConfiguration Performance { get; set; } = new();

    /// <summary>
    /// Gets or sets the exception handling configuration
    /// </summary>
    public ExceptionHandlingConfiguration ExceptionHandling { get; set; } = new();

    /// <summary>
    /// Gets or sets the filtering configuration
    /// </summary>
    public FilteringConfiguration Filtering { get; set; } = new();

    /// <summary>
    /// Gets or sets the async logging configuration
    /// </summary>
    public AsyncLoggingConfiguration AsyncLogging { get; set; } = new();
}

/// <summary>
/// Activity log levels
/// </summary>
public enum ActivityLogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}

/// <summary>
/// Serialization configuration options
/// </summary>
public class SerializationConfiguration
{
    /// <summary>
    /// Gets or sets the maximum depth for object serialization
    /// </summary>
    public int MaxDepth { get; set; } = 3;

    /// <summary>
    /// Gets or sets the maximum length for serialized strings
    /// </summary>
    public int MaxStringLength { get; set; } = 1000;

    /// <summary>
    /// Gets or sets whether to serialize input parameters
    /// </summary>
    public bool SerializeInputParameters { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to serialize return values
    /// </summary>
    public bool SerializeReturnValues { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to ignore null values during serialization
    /// </summary>
    public bool IgnoreNullValues { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of types to exclude from serialization
    /// </summary>
    public HashSet<string> ExcludedTypes { get; set; } =
    [
        "System.IO.Stream",
        "Microsoft.AspNetCore.Http.HttpContext",
        "Microsoft.Extensions.Logging.ILogger"
    ];

    /// <summary>
    /// Gets or sets the list of property names to exclude from serialization
    /// </summary>
    public HashSet<string> ExcludedProperties { get; set; } =
    [
        "Password",
        "Secret",
        "Token",
        "Key",
        "ConnectionString"
    ];
}

/// <summary>
/// Performance configuration options
/// </summary>
public class PerformanceConfiguration
{
    /// <summary>
    /// Gets or sets whether to enable high precision timing
    /// </summary>
    public bool EnableHighPrecisionTiming { get; set; } = true;

    /// <summary>
    /// Gets or sets the threshold in milliseconds for slow method detection
    /// </summary>
    public double SlowMethodThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets whether to log only slow methods
    /// </summary>
    public bool LogOnlySlowMethods { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of concurrent logging operations
    /// </summary>
    public int MaxConcurrentLoggingOperations { get; set; } = Environment.ProcessorCount * 2;
}

/// <summary>
/// Exception handling configuration options
/// </summary>
public class ExceptionHandlingConfiguration
{
    /// <summary>
    /// Gets or sets whether to capture full stack traces
    /// </summary>
    public bool CaptureFullStackTrace { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to capture inner exceptions
    /// </summary>
    public bool CaptureInnerExceptions { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to continue execution after logging failures
    /// </summary>
    public bool ContinueOnLoggingFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum exception message length
    /// </summary>
    public int MaxExceptionMessageLength { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed operations
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}

/// <summary>
/// Filtering configuration options
/// </summary>
public class FilteringConfiguration
{
    /// <summary>
    /// Gets or sets the list of namespaces to include in logging
    /// </summary>
    public HashSet<string> IncludedNamespaces { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of namespaces to exclude from logging
    /// </summary>
    public HashSet<string> ExcludedNamespaces { get; set; } =
    [
        "System",
        "Microsoft.Extensions.Logging",
        "Microsoft.EntityFrameworkCore"
    ];

    /// <summary>
    /// Gets or sets the list of method names to exclude from logging
    /// </summary>
    public HashSet<string> ExcludedMethods { get; set; } =
    [
        "ToString",
        "GetHashCode",
        "Equals",
        "GetType"
    ];

    /// <summary>
    /// Gets or sets whether to use whitelist mode (only log included namespaces)
    /// </summary>
    public bool UseWhitelistMode { get; set; } = false;
}

/// <summary>
/// Async logging configuration options
/// </summary>
public class AsyncLoggingConfiguration
{
    /// <summary>
    /// Gets or sets whether to enable async logging
    /// </summary>
    public bool EnableAsyncLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the batch size for async logging
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the flush interval in milliseconds
    /// </summary>
    public int FlushIntervalMs { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the maximum queue size for async logging
    /// </summary>
    public int MaxQueueSize { get; set; } = 10000;

    /// <summary>
    /// Gets or sets the timeout for async operations in milliseconds
    /// </summary>
    public int AsyncTimeoutMs { get; set; } = 30000;
}
