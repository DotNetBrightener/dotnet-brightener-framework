using DotNetBrightener.Core.Logging.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LogLevel = NLog.LogLevel;

namespace DotNetBrightener.Core.Logging;

public interface IQueueEventLogBackgroundProcessing
{
    Task Execute();
}

public class QueueEventLogBackgroundProcessing : IQueueEventLogBackgroundProcessing
{
    private readonly IEventLogWatcher  _eventLogWatcher;
    private readonly IServiceProvider  _serviceResolver;
    private readonly LoggingRetentions _loggingRetentions;
    private readonly ILogger           _logger;

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
        using var backgroundScope = _serviceResolver.CreateScope();

        try
        {
            var eventLogDataService = backgroundScope.ServiceProvider.GetService<IEventLogDataService>();

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
                LogLevel.Warn.ToString(), 
                Microsoft.Extensions.Logging.LogLevel.Warning.ToString()
            };

            string[] errorAndWarning = [..errorLogStrings, ..warningLogStrings];

            // delete error logs
            const string logMessage = "Removing {logLevel} logs which are older than {logTimestamp}";

            _logger.LogInformation(logMessage, LogLevel.Error, errorLogOld);
            eventLogDataService.DeleteMany(_ => errorLogStrings.Contains(_.Level) &&
                                                _.TimeStamp <= errorLogOld,
                                           forceHardDelete: true);

            // delete warning logs
            _logger.LogInformation(logMessage, LogLevel.Warn, warningLogOld);
            eventLogDataService.DeleteMany(_ => warningLogStrings.Contains(_.Level) &&
                                                _.TimeStamp <= warningLogOld,
                                           forceHardDelete: true);

            // delete other logs
            _logger.LogInformation(logMessage, "Other", defaultLogOld);
            eventLogDataService.DeleteMany(_ => !errorAndWarning.Contains(_.Level) &&
                                                _.TimeStamp <= defaultLogOld,
                                           forceHardDelete: true);
            
            var eventLogRecords = _eventLogWatcher.GetQueuedEventLogRecords();

            if (eventLogRecords.Count == 0)
                return;

            var dataToLog = eventLogRecords.Select(model => new EventLog(model))
                                           .ToList();

            await eventLogDataService!.InsertAsync(dataToLog);
        }
        catch
        {
            // ignore if cannot resolve log writer
        }
    }
}