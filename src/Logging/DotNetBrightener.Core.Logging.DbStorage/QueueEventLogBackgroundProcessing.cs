using DotNetBrightener.Core.Logging.DbStorage.Data;
using DotNetBrightener.Core.Logging.Options;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LogLevel = NLog.LogLevel;

namespace DotNetBrightener.Core.Logging.DbStorage;

internal class QueueEventLogBackgroundProcessing : IQueueEventLogBackgroundProcessing
{
    private readonly IEventLogWatcher  _eventLogWatcher;
    private readonly IServiceProvider  _serviceResolver;
    private readonly LoggingRetentions _loggingRetentions;

    private readonly ILogger _logger;

    // delete error logs
    private const string LogMessage = "Removing {logLevel} logs which are older than {logTimestamp}";

    public QueueEventLogBackgroundProcessing(IEventLogWatcher                           eventLogWatcher,
                                             IServiceProvider                           backgroundServiceProvider,
                                             IOptions<LoggingRetentions>                loggingRetentionOptions,
                                             ILogger<QueueEventLogBackgroundProcessing> logger)
    {
        _eventLogWatcher   = eventLogWatcher;
        _serviceResolver   = backgroundServiceProvider;
        _logger            = logger;
        _loggingRetentions = loggingRetentionOptions.Value;
    }

    public async Task Execute()
    {
        using var        backgroundScope  = _serviceResolver.CreateScope();
        LoggingDbContext loggingDbContext = null;

        try
        {
            loggingDbContext = backgroundScope.ServiceProvider.GetRequiredService<LoggingDbContext>();
        }
        catch
        {
            // ignore if cannot resolve log writer
        }

        if (loggingDbContext == null)
            return;

        await using (loggingDbContext)
        {
            var errorLogOld = DateTime.UtcNow.AddDays(-_loggingRetentions.ErrorRetentionsInDay);

            var warningLogOld = DateTime.UtcNow.AddDays(-_loggingRetentions.WarningRetentionsInDay);

            var defaultLogOld = DateTime.UtcNow.AddDays(-_loggingRetentions.DefaultRetentionsInDay);

            var errorLogStrings = new[]
                {
                    LogLevel.Error.ToString(),
                    LogLevel.Fatal.ToString(),
                    Microsoft.Extensions.Logging.LogLevel.Error.ToString(),
                    Microsoft.Extensions.Logging.LogLevel.Critical.ToString()
                }.Distinct()
                 .ToArray();

            var warningLogStrings = new[]
            {
                LogLevel.Warn.ToString(), Microsoft.Extensions.Logging.LogLevel.Warning.ToString()
            };

            string[] errorAndWarning = [.. errorLogStrings, .. warningLogStrings];


            _logger.LogDebug(LogMessage, LogLevel.Error, errorLogOld);

            var eventLogsTable = loggingDbContext.Set<EventLog>();

            await eventLogsTable
                 .Where(_ => errorLogStrings.Contains(_.Level) &&
                             _.TimeStamp <= errorLogOld)
                 .ExecuteDeleteAsync();

            // delete warning logs
            _logger.LogDebug(LogMessage, LogLevel.Warn, warningLogOld);

            await eventLogsTable
                 .Where(_ => warningLogStrings.Contains(_.Level) &&
                             _.TimeStamp <= warningLogOld)
                 .ExecuteDeleteAsync();

            // delete other logs
            _logger.LogDebug(LogMessage, "Other", defaultLogOld);

            await eventLogsTable
                 .Where(_ => !errorAndWarning.Contains(_.Level) &&
                             _.TimeStamp <= defaultLogOld)
                 .ExecuteDeleteAsync();

            var eventLogRecords = _eventLogWatcher.GetQueuedEventLogRecords();

            if (eventLogRecords.Count == 0)
                return;

            var dataToLog = eventLogRecords.Select(model => new EventLog(model))
                                           .ToList();

            try
            {
                await loggingDbContext!.BulkCopyAsync(dataToLog);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                                   "BulkInsert failed to insert {numberOfRecords} records entities of type {Type}. " +
                                   "Retrying with slow insert...",
                                   dataToLog.Count,
                                   nameof(EventLog));

                await eventLogsTable.AddRangeAsync(dataToLog);
                await loggingDbContext.SaveChangesAsync();
            }
        }
    }
}