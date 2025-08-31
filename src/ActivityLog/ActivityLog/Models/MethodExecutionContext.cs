using System.Diagnostics;
using System.Reflection;

namespace ActivityLog.Models;

/// <summary>
/// Represents the execution context of a method being logged
/// </summary>
public class MethodExecutionContext
{
    /// <summary>
    /// Gets or sets the unique identifier for this execution context
    /// </summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    /// Gets or sets the correlation ID for tracking related activities
    /// </summary>
    public Guid? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the method information
    /// </summary>
    public MethodInfo MethodInfo { get; set; } = null!;

    /// <summary>
    /// Gets or sets the target object instance (null for static methods)
    /// </summary>
    public object? Target { get; set; }

    /// <summary>
    /// Gets or sets the method arguments
    /// </summary>
    public object?[] Arguments { get; set; } = Array.Empty<object>();

    /// <summary>
    /// Gets or sets the method return value
    /// </summary>
    public object? ReturnValue { get; set; }

    /// <summary>
    /// Gets or sets the exception that occurred during method execution
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the start time of method execution
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of method execution
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the stopwatch for high-precision timing
    /// </summary>
    public Stopwatch Stopwatch { get; set; } = new();

    /// <summary>
    /// Gets or sets the activity name from the LogActivity attribute
    /// </summary>
    public string? ActivityName { get; set; }

    /// <summary>
    /// Gets or sets the activity description from the LogActivity attribute
    /// </summary>
    public string? ActivityDescription { get; set; }

    /// <summary>
    /// Gets or sets the description format from the LogActivity attribute
    /// </summary>
    public string? DescriptionFormat { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the execution
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the user information
    /// </summary>
    public UserContext? UserContext { get; set; }

    /// <summary>
    /// Gets or sets the HTTP context information (if available)
    /// </summary>
    public HttpContextInfo? HttpContext { get; set; }

    /// <summary>
    /// Gets the execution duration in milliseconds
    /// </summary>
    public double? ExecutionDurationMs => EndTime.HasValue 
        ? (EndTime.Value - StartTime).TotalMilliseconds 
        : Stopwatch.IsRunning 
            ? Stopwatch.Elapsed.TotalMilliseconds 
            : null;

    /// <summary>
    /// Gets whether the method execution was successful
    /// </summary>
    public bool IsSuccess => Exception == null;

    /// <summary>
    /// Gets the full method name including class and namespace
    /// </summary>
    public string FullMethodName => $"{MethodInfo.DeclaringType?.FullName}.{MethodInfo.Name}";

    /// <summary>
    /// Gets the class name
    /// </summary>
    public string? ClassName => MethodInfo.DeclaringType?.Name;

    /// <summary>
    /// Gets the namespace
    /// </summary>
    public string? Namespace => MethodInfo.DeclaringType?.Namespace;

    /// <summary>
    /// Starts the execution timing
    /// </summary>
    public void StartTiming()
    {
        StartTime = DateTimeOffset.UtcNow;
        Stopwatch.Start();
    }

    /// <summary>
    /// Stops the execution timing
    /// </summary>
    public void StopTiming()
    {
        EndTime = DateTimeOffset.UtcNow;
        Stopwatch.Stop();
    }
}

/// <summary>
/// Represents user context information
/// </summary>
public class UserContext
{
    /// <summary>
    /// Gets or sets the user ID
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// Gets or sets the username
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets additional user claims
    /// </summary>
    public Dictionary<string, string> Claims { get; set; } = new();
}

/// <summary>
/// Represents HTTP context information
/// </summary>
public class HttpContextInfo
{
    /// <summary>
    /// Gets or sets the request method
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    /// Gets or sets the request URL
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the user agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the IP address
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the request headers
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();
}

/// <summary>
/// Represents the result of a method execution logging operation
/// </summary>
public class LoggingResult
{
    /// <summary>
    /// Gets or sets whether the logging operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the error message if logging failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the exception that occurred during logging
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the time taken to perform the logging operation
    /// </summary>
    public TimeSpan LoggingDuration { get; set; }

    /// <summary>
    /// Creates a successful logging result
    /// </summary>
    public static LoggingResult Success(TimeSpan duration) => new()
    {
        IsSuccess = true,
        LoggingDuration = duration
    };

    /// <summary>
    /// Creates a failed logging result
    /// </summary>
    public static LoggingResult Failure(string errorMessage, Exception? exception = null, TimeSpan duration = default) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Exception = exception,
        LoggingDuration = duration
    };
}
