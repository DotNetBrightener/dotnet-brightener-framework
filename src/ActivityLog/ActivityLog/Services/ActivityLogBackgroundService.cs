using ActivityLog.Configuration;
using ActivityLog.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ActivityLog.Services;

/// <summary>
///     Background service responsible for processing queued activity log entries
/// </summary>
internal class ActivityLogBackgroundService(
    IOptions<ActivityLogConfiguration>    configuration,
    IServiceScopeFactory                  serviceScopeFactory,
    ILogger<ActivityLogBackgroundService> logger,
    IActivityLogQueueAccessor             queueAccessor,
    ActivityLogContextAccessorInitializer initializer)
    : BackgroundService
{
    private readonly ActivityLogConfiguration _configuration = configuration.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.AsyncLogging.EnableAsyncLogging)
        {
            logger.LogInformation("Async logging is disabled. ActivityLogBackgroundService will not process entries.");

            return;
        }

        logger.LogInformation("ActivityLogBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueuedLogsAsync(stoppingToken);

                // Wait for the configured flush interval before processing again
                await Task.Delay(
                                 TimeSpan.FromMilliseconds(_configuration.AsyncLogging.FlushIntervalMs),
                                 stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing activity log queue");

                // Wait a bit before retrying to avoid tight error loops
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        logger.LogInformation("ActivityLogBackgroundService stopped");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("ActivityLogBackgroundService is stopping. Processing remaining entries...");

        // Process any remaining entries before stopping
        try
        {
            await ProcessQueuedLogsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during final queue processing");
        }

        await base.StopAsync(cancellationToken);
    }

    private async Task ProcessQueuedLogsAsync(CancellationToken cancellationToken)
    {
        var batch     = new List<ActivityLogRecord>();
        var batchSize = _configuration.AsyncLogging.BatchSize;

        // Dequeue items for batch processing
        while (batch.Count < batchSize &&
               queueAccessor.TryDequeue(out var logEntry))
        {
            batch.Add(logEntry);
        }

        if (batch.Count == 0)
            return;

        logger.LogDebug("Processing batch of {Count} activity log entries", batch.Count);

        var retryCount = 0;
        var maxRetries = _configuration.ExceptionHandling.MaxRetryAttempts;

        while (retryCount <= maxRetries)
        {
            try
            {
                await PersistLogEntriesBatchAsync(batch, cancellationToken);
                logger.LogDebug("Successfully persisted {Count} activity log entries", batch.Count);

                return; // Success, exit retry loop
            }
            catch (Exception ex) when (retryCount < maxRetries)
            {
                retryCount++;
                logger.LogWarning(ex,
                                  "Failed to persist batch of {Count} activity log entries. Retry attempt {RetryCount}/{MaxRetries}",
                                  batch.Count,
                                  retryCount,
                                  maxRetries);

                // Exponential backoff for retries
                var delay = TimeSpan.FromMilliseconds(Math.Pow(2, retryCount) * 1000);
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                                "Failed to persist batch of {Count} activity log entries after {MaxRetries} retries",
                                batch.Count,
                                maxRetries);

                // Re-queue failed entries if configured to continue on failure
                if (_configuration.ExceptionHandling.ContinueOnLoggingFailure)
                {
                    foreach (var entry in batch)
                    {
                        queueAccessor.Enqueue(entry);
                    }

                    logger.LogWarning("Re-queued {Count} failed entries for later processing", batch.Count);
                }

                break; // Exit retry loop
            }
        }
    }

    private async Task PersistLogEntriesBatchAsync(List<ActivityLogRecord> activityLogs,
                                                   CancellationToken       cancellationToken)
    {
        using var scope      = serviceScopeFactory.CreateScope();
        var       repository = scope.ServiceProvider.GetService<IActivityLogRepository>();

        if (repository == null)
        {
            logger.LogError("IActivityLogRepository is not registered in the service container");

            throw new InvalidOperationException("IActivityLogRepository is not registered in the service container");
        }

        await repository.InsertManyAsync(activityLogs);
        await repository.CommitChangesAsync();
    }
}