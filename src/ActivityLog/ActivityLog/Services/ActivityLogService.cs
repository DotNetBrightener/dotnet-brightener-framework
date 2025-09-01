using ActivityLog.Configuration;
using ActivityLog.Entities;
using ActivityLog.Internal;
using ActivityLog.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace ActivityLog.Services;

/// <summary>
/// Main implementation of IActivityLogService with async logging capabilities
/// </summary>
public class ActivityLogService : IActivityLogService, IDisposable
{
    private readonly ActivityLogConfiguration    _configuration;
    private readonly IServiceScopeFactory        _serviceScopeFactory;
    private readonly IActivityLogSerializer      _serializer;
    private readonly ILogger<ActivityLogService> _logger;

    private readonly ConcurrentQueue<ActivityLogRecord> _logQueue       = new();
    private readonly SemaphoreSlim                      _processingLock = new(1, 1);
    private readonly Timer                              _flushTimer;
    private readonly CancellationTokenSource            _cancellationTokenSource = new();

    private volatile bool  _disposed;
    private          Task? _backgroundTask;

    public ActivityLogService(IServiceScopeFactory               serviceScopeFactory,
                              IActivityLogSerializer             serializer,
                              IOptions<ActivityLogConfiguration> configuration,
                              ILogger<ActivityLogService>        logger)
    {
        _configuration       = configuration.Value;
        _serviceScopeFactory = serviceScopeFactory;
        _serializer          = serializer;
        _logger              = logger;

        // Initialize flush timer if async logging is enabled
        if (_configuration.AsyncLogging.EnableAsyncLogging)
        {
            _flushTimer = new Timer(
                                    FlushTimerCallback,
                                    null,
                                    TimeSpan.FromMilliseconds(_configuration.AsyncLogging.FlushIntervalMs),
                                    TimeSpan.FromMilliseconds(_configuration.AsyncLogging.FlushIntervalMs));
        }
        else
        {
            _flushTimer = null!;
        }
    }

    public async Task<LoggingResult> LogMethodCompletionAsync(MethodExecutionContext context)
    {
        if (!_configuration.IsEnabled)
            return LoggingResult.Success(TimeSpan.Zero);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var activityLog = CreateActivityLogEntity(context, isStart: false);
            await EnqueueLogEntryAsync(activityLog);

            stopwatch.Stop();

            return LoggingResult.Success(stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to log method completion for {MethodName}", context.FullMethodName);

            return LoggingResult.Failure("Failed to log method completion", ex, stopwatch.Elapsed);
        }
    }

    public async Task<LoggingResult> LogMethodFailureAsync(MethodExecutionContext context)
    {
        if (!_configuration.IsEnabled)
            return LoggingResult.Success(TimeSpan.Zero);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var activityLog = CreateActivityLogEntity(context, isStart: false);
            await EnqueueLogEntryAsync(activityLog);

            stopwatch.Stop();

            return LoggingResult.Success(stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to log method failure for {MethodName}", context.FullMethodName);

            return LoggingResult.Failure("Failed to log method failure", ex, stopwatch.Elapsed);
        }
    }

    public async Task FlushAsync()
    {
        if (_disposed || !_configuration.AsyncLogging.EnableAsyncLogging)
            return;

        await _processingLock.WaitAsync();

        try
        {
            await ProcessQueuedLogsAsync();
        }
        finally
        {
            _processingLock.Release();
        }
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
                            : DateTimeOffset.FromUnixTimeMilliseconds(context.StartTimestamp!.Value),
            EndTime = context.EndTimestamp is null
                          ? DateTimeOffset.Now
                          : DateTimeOffset.FromUnixTimeMilliseconds(context.EndTimestamp!.Value),
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
            activityLog.InputParameters = _serializer.SerializeMetadata(context.Arguments);
        }

        if (!isStart &&
            _configuration.Serialization.SerializeReturnValues &&
            context.ReturnValue != null)
        {
            activityLog.ReturnValue = _serializer.SerializeReturnValue(context.ReturnValue);
        }

        // Handle exceptions
        if (context.Exception != null)
        {
            activityLog.Exception     = _serializer.SerializeException(context.Exception);
            activityLog.ExceptionType = context.Exception.GetType().FullName;
        }

        // Add metadata
        if (context.Metadata.Count > 0)
        {
            activityLog.Metadata = _serializer.SerializeMetadata(context.Metadata);
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
                        var value = kvp.Value?.ToString();
                        
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
                        var value = kvp.Value?.ToString();
                        
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

    private async Task EnqueueLogEntryAsync(ActivityLogRecord activityLogRecord)
    {
        // Check queue size limit
        if (_logQueue.Count >= _configuration.AsyncLogging.MaxQueueSize)
        {
            _logger.LogWarning("Activity log queue is full. Dropping log entry for {MethodName}",
                               activityLogRecord.MethodName);

            return;
        }

        _logQueue.Enqueue(activityLogRecord);
    }

    private async Task ProcessQueuedLogsAsync()
    {
        var batch     = new List<ActivityLogRecord>();
        var batchSize = _configuration.AsyncLogging.BatchSize;

        // Dequeue items for batch processing
        while (batch.Count < batchSize &&
               _logQueue.TryDequeue(out var logEntry))
        {
            batch.Add(logEntry);
        }

        if (batch.Count == 0)
            return;

        try
        {
            await PersistLogEntriesBatchAsync(batch);
            _logger.LogDebug("Successfully persisted {Count} activity log entries", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist batch of {Count} activity log entries", batch.Count);

            // Re-queue failed entries if configured to do so
            if (_configuration.ExceptionHandling.ContinueOnLoggingFailure)
            {
                foreach (var entry in batch)
                {
                    _logQueue.Enqueue(entry);
                }
            }
        }
    }

    private async Task PersistLogEntriesBatchAsync(List<ActivityLogRecord> activityLogs)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var repository = scope.ServiceProvider.GetService<IActivityLogRepository>();

        await repository!.InsertManyAsync(activityLogs);
        await repository.CommitChangesAsync();
    }

    private void FlushTimerCallback(object? state)
    {
        if (_disposed)
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                await FlushAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled flush");
            }
        });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_configuration.AsyncLogging.EnableAsyncLogging)
        {
            _backgroundTask = Task.Run(BackgroundProcessingAsync, cancellationToken);
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cancellationTokenSource.CancelAsync();

        if (_backgroundTask != null)
        {
            await _backgroundTask;
        }

        // Final flush
        await FlushAsync();
    }

    private async Task BackgroundProcessingAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, _cancellationTokenSource.Token);
                await FlushAsync();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background processing");
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _cancellationTokenSource.Cancel();
        _flushTimer?.Dispose();
        _processingLock.Dispose();
        _cancellationTokenSource.Dispose();
    }
}