using DotNetBrightener.Core.Logging.Options;
using DotNetBrightener.Core.Logging.PostgreSqlDbStorage.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DotNetBrightener.Core.Logging.PostgreSqlDbStorage;

internal class NpgsqlQueueEventLogBackgroundProcessing(
    IEventLogWatcher                           eventLogWatcher,
    IOptions<LoggingRetentions>                loggingRetentionOptions,
    ILogger<NpgsqlQueueEventLogBackgroundProcessing> logger,
    IServiceScopeFactory                       serviceScopeFactory)
    : IQueueEventLogBackgroundProcessing
{
    /// <summary>
    ///     To keep track of whether the migration has executed or not.
    /// </summary>
    private static   bool              _migrationHasExecuted;

    private readonly LoggingRetentions _loggingRetentions = loggingRetentionOptions.Value;
    private readonly ILogger           _logger            = logger;

    public async Task Execute()
    {
        await ExecuteLogSchemaMigrationIfNeeded();

        var taskList = new List<Func<LoggingDbContext, Task>>
        {
            context =>
                CleanUpLogsByLevel(context,
                                   TimeSpan.FromDays(_loggingRetentions.ErrorRetentionsInDay),
                                   ["Error", "Fatal", "Critical"]),
            context =>
                CleanUpLogsByLevel(context,
                                   TimeSpan.FromDays(_loggingRetentions.WarningRetentionsInDay),
                                   ["Warning", "Warn"]),
            context => CleanUpLogsByLevel(context, _loggingRetentions.DefaultRetentions, ["Info", "Information"]),
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

    private async Task ExecuteLogSchemaMigrationIfNeeded()
    {
        if (!_migrationHasExecuted)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                await using (var dbContext = scope.ServiceProvider.GetRequiredService<LoggingDbContext>())
                {
                    dbContext.AutoMigrateDbSchema(_logger);
                }
            }

            _migrationHasExecuted = true;
        }
    }

    private async Task CleanUpLogsByLogger(LoggingDbContext context, TimeSpan retention, string loggerName)
    {
        var retentionStartDate = DateTime.UtcNow.Subtract(retention);

        _logger.LogInformation("Deleting logs from logger {logger}", loggerName);

        Expression<Func<EventLog, bool>> loggerFilterExpression;

        var loggerNameActuallyName = loggerName.Trim('*')
                                               .Trim();

        if (loggerName.StartsWith("*") &&
            loggerName.EndsWith("*"))
        {
            loggerFilterExpression = eventLog => eventLog.LoggerName.Contains(loggerNameActuallyName);
        }
        else if (loggerName.StartsWith("*"))
        {
            loggerFilterExpression = eventLog => eventLog.LoggerName.EndsWith(loggerNameActuallyName);
        }
        else if (loggerName.EndsWith("*"))
        {
            loggerFilterExpression = eventLog => eventLog.LoggerName.StartsWith(loggerNameActuallyName);
        }
        else
        {
            loggerFilterExpression = eventLog => eventLog.LoggerName == loggerName;
        }

        loggerFilterExpression = loggerFilterExpression.And(eventLog => eventLog.TimeStamp <= retentionStartDate &&
                                                                        (eventLog.Level == "Info" ||
                                                                         eventLog.Level == "Information"));

        await context.Set<EventLog>()
                     .Where(loggerFilterExpression)
                     .ExecuteDeleteAsync();
    }

    private async Task CleanUpLogsByLevel(LoggingDbContext context,
                                          TimeSpan         retentions,
                                          params string[]  logLevelsToDelete)
    {
        var retentionStartDate = DateTime.UtcNow.Subtract(retentions);

        try
        {
            await context.Set<EventLog>()
                         .Where(eventLog => logLevelsToDelete.Contains(eventLog.Level) &&
                                            eventLog.TimeStamp <= retentionStartDate)
                         .ExecuteDeleteAsync();
        }
        catch (Exception exception)
        {
            var fullExceptionMessage = exception.GetFullExceptionMessage();

            if (fullExceptionMessage.Contains("Failed executing DbCommand") &&
                fullExceptionMessage.Contains("OPENJSON"))
            {
                _logger.LogInformation(exception,
                                       "Failed to delete {logLevel} logs which are older than {logTimestamp}. " +
                                       "Retrying with slow delete...",
                                       logLevelsToDelete,
                                       retentionStartDate);

                foreach (var logLevel in logLevelsToDelete)
                {
                    await context.Set<EventLog>()
                                 .Where(eventLog => eventLog.Level == logLevel &&
                                                    eventLog.TimeStamp <= retentionStartDate)
                                 .ExecuteDeleteAsync();
                }
            }
        }
    }

    private async Task WriteNewLogs(LoggingDbContext loggingDbContext)
    {
        var eventLogRecords = eventLogWatcher.GetQueuedEventLogRecords();

        if (eventLogRecords.Count == 0)
            return;

        var dataToLog = eventLogRecords.Select(model => new EventLog(model))
                                       .ToList();

        try
        {
            await loggingDbContext.Set<EventLog>()
                                  .AddRangeAsync(dataToLog);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while saving logs");
        }
    }

    private async Task UsingDbContext(Func<LoggingDbContext, Task> action)
    {
        using var        backgroundScope  = serviceScopeFactory.CreateScope();
        LoggingDbContext loggingDbContext = null;

        try
        {
            var dbContext = backgroundScope.ServiceProvider
                                              .GetRequiredService<LoggingDbContext>();

            // fake call to ensure db connection is working

            var logCount = await dbContext.Set<EventLog>()
                                          .CountAsync();

            loggingDbContext = dbContext;
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