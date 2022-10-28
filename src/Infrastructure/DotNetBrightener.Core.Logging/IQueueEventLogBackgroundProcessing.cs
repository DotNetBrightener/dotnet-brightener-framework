using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Core.Logging;

public interface IQueueEventLogBackgroundProcessing
{
    Task Execute();
}

public class QueueEventLogBackgroundProcessing : IQueueEventLogBackgroundProcessing
{
    private readonly IEventLogWatcher _eventLogWatcher;
    private readonly IServiceProvider _serviceResolver;

    public QueueEventLogBackgroundProcessing(IEventLogWatcher eventLogWatcher,
                                             IServiceProvider backgroundServiceProvider)
    {
        _eventLogWatcher = eventLogWatcher;
        _serviceResolver = backgroundServiceProvider;
    }

    public async Task Execute()
    {
        var eventLogRecords = _eventLogWatcher.GetQueuedEventLogRecords();

        if (eventLogRecords.Count == 0)
            return;


        using var backgroundScope     = _serviceResolver.CreateScope();
        var       eventLogDataService = backgroundScope.ServiceProvider.GetService<IEventLogDataService>();

        var dataToLog = eventLogRecords.Select(_ => new EventLog(_))
                                       .ToList();

        await eventLogDataService!.InsertAsync(dataToLog);
    }
}