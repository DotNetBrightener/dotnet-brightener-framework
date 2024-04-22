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

            _logger.LogDebug(LogMessage, LogLevel.Error, errorLogOld);

            var eventLogsTable = loggingDbContext.Set<EventLog>();

            await eventLogsTable
                 .Where(eventLog => (eventLog.Level == "Error" || eventLog.Level == "Fatal" ||
                                     eventLog.Level == "Critical") &&
                                    eventLog.TimeStamp <= errorLogOld)
                 .ExecuteDeleteAsync();

            // delete warning logs
            _logger.LogDebug(LogMessage, LogLevel.Warn, warningLogOld);

            await eventLogsTable
                 .Where(eventLog => (eventLog.Level == "Warn" || eventLog.Level == "Warning") &&
                                    eventLog.TimeStamp <= warningLogOld)
                 .ExecuteDeleteAsync();

            // delete other logs
            _logger.LogDebug(LogMessage, "Other", defaultLogOld);

            await eventLogsTable
                 .Where(eventLog => (eventLog.Level == "Info" || eventLog.Level == "Information") &&
                                    eventLog.TimeStamp <= defaultLogOld)
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
                try
                {

                    _logger.LogWarning(ex,
                                       "BulkInsert failed to insert {numberOfRecords} records entities of type {Type}. " +
                                       "Retrying with slow insert...",
                                       dataToLog.Count,
                                       nameof(EventLog));

                    await eventLogsTable.AddRangeAsync(dataToLog);
                    await loggingDbContext.SaveChangesAsync();
                }
                catch
                {
                    // just ignore
                }
            }
        }
    }
}