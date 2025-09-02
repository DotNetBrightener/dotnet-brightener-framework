using ActivityLog.Configuration;
using ActivityLog.Entities;
using ActivityLog.Internal;
using ActivityLog.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ActivityLog.Services;

public class ActivityLogService(
    IActivityLogSerializer             serializer,
    IOptions<ActivityLogConfiguration> configuration,
    ILogger<ActivityLogService>        logger)
    : IActivityLogService, IActivityLogQueueAccessor, IDisposable
{
    private readonly ActivityLogConfiguration           _configuration = configuration.Value;
    private readonly ConcurrentQueue<ActivityLogRecord> _logQueue      = new();
    private volatile bool                               _disposed;

    public Task LogMethodCompletionAsync(MethodExecutionContext context)
    {
        if (!_configuration.IsEnabled)
            return Task.CompletedTask;

        try
        {
            var activityLog = CreateActivityLogEntity(context, isStart: false);
            EnqueueLogEntry(activityLog);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log method completion for {MethodName}", context.FullMethodName);
        }

        return Task.CompletedTask;
    }

    public Task LogMethodFailureAsync(MethodExecutionContext context)
    {
        if (!_configuration.IsEnabled)
            return Task.CompletedTask;

        try
        {
            var activityLog = CreateActivityLogEntity(context, isStart: false);
            EnqueueLogEntry(activityLog);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log method failure for {MethodName}", context.FullMethodName);
        }

        return Task.CompletedTask;
    }

    private ActivityLogRecord CreateActivityLogEntity(MethodExecutionContext context,
                                                      bool                   isStart)
    {
        var activityLog = new ActivityLogRecord
        {
            Id                  = Guid.CreateVersion7(),
            ActivityName        = context.ActivityName ?? context.MethodInfo.Name,
            TargetEntity        = context.TargetEntity,
            ActivityDescription = FormatDescription(context),
            StartTime = context.StartTimestamp is null
                            ? DateTimeOffset.Now
                            : ConvertToDateTimeOffset(context.StartTimestamp.Value),
            EndTime = context.EndTimestamp is null
                          ? DateTimeOffset.Now
                          : ConvertToDateTimeOffset(context.EndTimestamp.Value),
            ExecutionDurationMs = isStart ? null : context.ExecutionDurationMs,
            MethodName          = context.FullMethodName,
            ClassName           = context.ClassName,
            Namespace           = context.Namespace,
            IsSuccess           = context.IsSuccess,
            CorrelationId       = context.CorrelationId,
            LogLevel            = DetermineLogLevel(context).ToString(),
            Tags                = GenerateTags(context)
        };

        // Add user context
        if (context.UserContext != null)
        {
            activityLog.UserId   = context.UserContext.UserId;
            activityLog.UserName = context.UserContext.UserName;
        }

        // Add HTTP context
        if (context.HttpContext != null)
        {
            activityLog.UserAgent = context.HttpContext.UserAgent;
            activityLog.IpAddress = context.HttpContext.IpAddress;
        }

        // Serialize parameters and return value
        if (!isStart &&
            _configuration.Serialization.SerializeInputParameters)
        {
            activityLog.InputParameters = serializer.SerializeMetadata(context.Arguments);
        }

        if (!isStart &&
            _configuration.Serialization.SerializeReturnValues &&
            context.ReturnValue != null)
        {
            activityLog.ReturnValue = serializer.SerializeReturnValue(context.ReturnValue);
        }

        // Handle exceptions
        if (context.Exception != null)
        {
            activityLog.Exception     = serializer.SerializeException(context.Exception);
            activityLog.ExceptionType = context.Exception.GetType().FullName;
        }

        // Add metadata
        if (context.Metadata.Count > 0)
        {
            activityLog.Metadata = serializer.SerializeMetadata(context.Metadata);
        }

        return activityLog;
    }

    private string FormatDescription(MethodExecutionContext context)
    {
        if (!string.IsNullOrEmpty(context.ActivityDescription))
            return context.ActivityDescription;

        if (!string.IsNullOrEmpty(context.DescriptionFormat))
        {
            try
            {
                // Handle named arguments dictionary
                var formatDescription = context.DescriptionFormat;

                if (context.Arguments is { } namedArguments)
                {
                    // Replace named placeholders (e.g., {id}, {name})
                    foreach (var kvp in namedArguments)
                    {
                        var placeholder = "{" + kvp.Key + "}";
                        var value       = kvp.Value?.ToString();

                        if (value is not null)
                            formatDescription = formatDescription.Replace(placeholder, value);
                    }
                }

                if (context.Metadata is { } metadata)
                {
                    // Replace named placeholders (e.g., {id}, {name})
                    foreach (var kvp in metadata)
                    {
                        var placeholder = "{Metadata." + kvp.Key + "}";
                        var value       = kvp.Value?.ToString();

                        if (value is not null)
                            formatDescription = formatDescription.Replace(placeholder, value);
                    }
                }


                return formatDescription;
            }
            catch
            {
                return context.DescriptionFormat;
            }
        }

        return $"Executed method {context.FullMethodName}";
    }

    private ActivityLogLevel DetermineLogLevel(MethodExecutionContext context)
    {
        if (context.Exception != null)
            return ActivityLogLevel.Error;

        if (context.ExecutionDurationMs > _configuration.Performance.SlowMethodThresholdMs)
            return ActivityLogLevel.Warning;

        return ActivityLogLevel.Information;
    }

    private string GenerateTags(MethodExecutionContext context)
    {
        var tags = new List<string>();

        if (context.Exception != null)
            tags.Add("error");

        if (context.ExecutionDurationMs > _configuration.Performance.SlowMethodThresholdMs)
            tags.Add("slow");

        if (context.MethodInfo.IsStatic)
            tags.Add("static");

        if (context.MethodInfo.IsAsyncMethod())
            tags.Add("async");

        return string.Join(",", tags);
    }

    private void EnqueueLogEntry(ActivityLogRecord activityLogRecord)
    {
        // Check queue size limit
        if (_logQueue.Count >= _configuration.AsyncLogging.MaxQueueSize)
        {
            logger.LogWarning("Activity log queue is full. Dropping log entry for {MethodName}",
                              activityLogRecord.MethodName);

            return;
        }

        _logQueue.Enqueue(activityLogRecord);
    }

    /// <summary>
    /// Converts a Stopwatch timestamp to DateTimeOffset.
    /// Since Stopwatch timestamps are relative to system start time, we calculate the actual time
    /// by getting the elapsed time from the current timestamp and subtracting it from the current time.
    /// </summary>
    /// <param name="stopwatchTimestamp">The Stopwatch timestamp value</param>
    /// <returns>The corresponding DateTimeOffset</returns>
    private static DateTimeOffset ConvertToDateTimeOffset(long stopwatchTimestamp)
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var elapsedTime      = Stopwatch.GetElapsedTime(stopwatchTimestamp, currentTimestamp);

        return DateTimeOffset.UtcNow.Subtract(elapsedTime);
    }

    // IActivityLogQueueAccessor implementation
    public void Enqueue(ActivityLogRecord record)
    {
        EnqueueLogEntry(record);
    }

    public bool TryDequeue(out ActivityLogRecord record)
    {
        return _logQueue.TryDequeue(out record!);
    }

    public int Count => _logQueue.Count;

    public bool IsEmpty => _logQueue.IsEmpty;

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
    }
}