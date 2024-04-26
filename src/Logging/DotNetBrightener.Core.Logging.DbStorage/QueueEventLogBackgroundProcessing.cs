using System.Diagnostics;
using DotNetBrightener.Core.Logging.DbStorage.Data;
using DotNetBrightener.Core.Logging.Options;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Core.Logging.DbStorage;

internal class QueueEventLogBackgroundProcessing : IQueueEventLogBackgroundProcessing
{
    private readonly IEventLogWatcher     _eventLogWatcher;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly LoggingRetentions    _loggingRetentions;

    private readonly ILogger _logger;

    // delete error logs
    private const string LogMessage = "Removing {logLevel} logs which are older than {logTimestamp}";

    public QueueEventLogBackgroundProcessing(IEventLogWatcher                           eventLogWatcher,
                                             IOptions<LoggingRetentions>                loggingRetentionOptions,
                                             ILogger<QueueEventLogBackgroundProcessing> logger,
                                             IServiceScopeFactory                       serviceScopeFactory)
    {
        _eventLogWatcher     = eventLogWatcher;
        _logger              = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _loggingRetentions   = loggingRetentionOptions.Value;
    }

    public async Task Execute()
    {
        var taskList = new List<Func<LoggingDbContext, Task>>
        {
            context =>
                CleanUpLogsByLevel(context, _loggingRetentions.ErrorRetentionsInDay, ["Error", "Fatal", "Critical"]),
            context => CleanUpLogsByLevel(context, _loggingRetentions.WarningRetentionsInDay, ["Warning", "Warn"]),
            context => CleanUpLogsByLevel(context, _loggingRetentions.DefaultRetentionsInDay, ["Info", "Information"]),
            WriteNewLogs,
        };

        foreach (var loggingRetentionsLoggerRule in _loggingRetentions.LoggerRules)
        {
            taskList.Add((context) => CleanUpLogsByLogger(context,
                                                          loggingRetentionsLoggerRule.Value,
                                                          loggingRetentionsLoggerRule.Key));
        }

        Stopwatch sw = Stopwatch.StartNew();

        var allTasks = taskList.Select(UsingDbContext);

        await allTasks.WhenAll();

        sw.Stop();

        _logger.LogInformation("Event Log Queue Background Service executed in {elapsedTime}", sw.Elapsed);
    }

    private async Task CleanUpLogsByLogger(LoggingDbContext context, TimeSpan retention, string loggerName)
    {
        var retentionStartDate = DateTime.UtcNow.Subtract(retention);

        await context.Set<EventLog>()
                     .Where(eventLog => eventLog.LoggerName == loggerName &&
                                        eventLog.TimeStamp <= retentionStartDate)
                     .ExecuteDeleteAsync();
    }

    private async Task CleanUpLogsByLevel(LoggingDbContext context, int retentionsInDays, params string[] logLevelsToDelete)
    {
        var retentionStartDate = DateTime.UtcNow.AddDays(-retentionsInDays);

        await context.Set<EventLog>()
                     .Where(eventLog => logLevelsToDelete.Contains(eventLog.Level) &&
                                        eventLog.TimeStamp <= retentionStartDate)
                     .ExecuteDeleteAsync();
    }

    private async Task WriteNewLogs(LoggingDbContext loggingDbContext)
    {
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

                await loggingDbContext.Set<EventLog>()
                                      .AddRangeAsync(dataToLog);
            }
            catch
            {
                // just ignore
            }
        }
    }

    private async Task UsingDbContext(Func<LoggingDbContext, Task> action)
    {
        using var        backgroundScope  = _serviceScopeFactory.CreateScope();
        LoggingDbContext loggingDbContext = null;

        try
        {
            loggingDbContext = backgroundScope.ServiceProvider
                                              .GetRequiredService<LoggingDbContext>();
        }
        catch
        {
            // ignore if cannot resolve log writer
        }

        if (loggingDbContext == null)
            return;

        await using (loggingDbContext)
        {
            await action(loggingDbContext);

            await loggingDbContext.SaveChangesAsync();
        }
    }
}