using System.Diagnostics;
using System.Reflection;

namespace ActivityLog.Models;

/// <summary>
/// Represents the execution context of a method being logged
/// </summary>
public class MethodExecutionContext
{
    /// <summary>
    /// Gets or sets the correlation ID for tracking related activities
    /// </summary>
    public Guid? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the method information
    /// </summary>
    public MethodInfo MethodInfo { get; set; } = null!;

    /// <summary>
    /// Gets or sets the target entity id
    /// </summary>
    public string? TargetEntityId { get; set; }

    /// <summary>
    /// Gets or sets the method arguments
    /// </summary>
    public Dictionary<string, object?> Arguments { get; set; } = new();

    /// <summary>
    /// Gets or sets the method return value
    /// </summary>
    public object? ReturnValue { get; set; }

    /// <summary>
    /// Gets or sets the exception that occurred during method execution
    /// </summary>
    public Exception? Exception { get; set; }
    
    public long? StartTimestamp { get; set; }
    
    public long? EndTimestamp { get; set; }

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
    public double? ExecutionDurationMs => StartTimestamp is null || EndTimestamp is null
                                              ? 0
                                              : Stopwatch.GetElapsedTime(StartTimestamp!.Value, EndTimestamp!.Value)
                                                         .TotalMilliseconds;

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

    public string TargetEntity { get; set; }

    /// <summary>
    /// Starts the execution timing
    /// </summary>
    public void StartTiming()
    {
        StartTimestamp = Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// Stops the execution timing
    /// </summary>
    public void StopTiming()
    {
        EndTimestamp = Stopwatch.GetTimestamp();
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


