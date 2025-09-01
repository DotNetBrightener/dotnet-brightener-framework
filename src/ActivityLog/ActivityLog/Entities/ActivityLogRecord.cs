using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActivityLog.Entities;

public class ActivityLogRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>
    ///     The name of the activity
    /// </summary>
    public string ActivityName { get; set; }

    /// <summary>
    ///     The description of the activity
    /// </summary>
    public string ActivityDescription { get; set; }

    /// <summary>
    ///     The id of user who performs the activity
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    ///     The name of user who performs the activity
    /// </summary>
    [MaxLength(256)]
    public string? UserName { get; set; }

    /// <summary>
    ///     The target entity of the activity
    /// </summary>
    [MaxLength(256)]
    public string? TargetEntity { get; set; }

    /// <summary>
    ///     The id of the target entity of the activity
    /// </summary>
    [MaxLength(128)]
    public string? TargetEntityId { get; set; }

    /// <summary>
    ///     Start time of the activity with high precision
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    ///     End time of the activity with high precision
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    ///     Method execution duration in milliseconds for performance analysis
    /// </summary>
    public double? ExecutionDurationMs { get; set; }

    /// <summary>
    ///     The full method name including class and namespace
    /// </summary>
    [MaxLength(512)]
    public string? MethodName { get; set; }

    /// <summary>
    ///     The class name where the method is defined
    /// </summary>
    [MaxLength(256)]
    public string? ClassName { get; set; }

    /// <summary>
    ///     The namespace of the class
    /// </summary>
    [MaxLength(256)]
    public string? Namespace { get; set; }

    /// <summary>
    ///     Serialized input parameters of the method
    /// </summary>
    public string? InputParameters { get; set; }

    /// <summary>
    ///     Serialized return value of the method
    /// </summary>
    public string? ReturnValue { get; set; }

    /// <summary>
    ///     The exception details if the activity failed (includes full stack trace)
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    ///     The exception type if the activity failed
    /// </summary>
    [MaxLength(256)]
    public string? ExceptionType { get; set; }

    /// <summary>
    ///     Indicates whether the method execution was successful
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    ///     The metadata of the log activity
    /// </summary>
    [MaxLength(2048)]
    public string? Metadata { get; set; }

    [MaxLength(1024)]
    public string? UserAgent { get; set; }

    [MaxLength(128)]
    public string? IpAddress { get; set; }

    /// <summary>
    ///     Correlation ID for tracking related activities
    /// </summary>
    public Guid? CorrelationId { get; set; }

    /// <summary>
    ///     The logging level for this activity
    /// </summary>
    [MaxLength(32)]
    public string? LogLevel { get; set; }

    /// <summary>
    ///     Additional tags for categorization and filtering
    /// </summary>
    [MaxLength(512)]
    public string? Tags { get; set; }
}