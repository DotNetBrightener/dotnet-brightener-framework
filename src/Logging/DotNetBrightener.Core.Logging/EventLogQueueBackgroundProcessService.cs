#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.Logging;

public class EventLogQueueBackgroundProcessService : IHostedService, IDisposable
{
    private readonly ILogger<EventLogQueueBackgroundProcessService> _logger;
    private readonly IServiceScopeFactory                           _serviceScopeFactory;
    private          Timer?                                         _timer;
    private          bool                                           _isProcessing;

    public EventLogQueueBackgroundProcessService(ILogger<EventLogQueueBackgroundProcessService> logger,
                                                 IServiceScopeFactory                           serviceScopeFactory)
    {
        _logger              = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Starting Event Log Queue Collector Service.");

        _timer = new Timer(DoWork,
                           _serviceScopeFactory,
                           TimeSpan.FromSeconds(10),
                           TimeSpan.FromSeconds(10));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        if (state is not IServiceScopeFactory serviceScopeFactory ||
            _isProcessing)
        {
            return;
        }

        _logger.LogDebug("Prevent logger from executing until finish this execution");
        _isProcessing = _timer!.Change(Timeout.Infinite, 0);
        var shouldReEnable = true;

        using var scope = serviceScopeFactory.CreateScope();

        try
        {
            var queueEventLogBackgroundProcessing = scope.ServiceProvider
                                                         .GetRequiredService<
                                                              IQueueEventLogBackgroundProcessing>();

            if (queueEventLogBackgroundProcessing is NullEventLogBackgroundProcessing)
            {
                _logger.LogWarning("QueueEventLogBackgroundProcessing is not registered in the service provider.");
                shouldReEnable = false;

                return;
            }

            queueEventLogBackgroundProcessing.Execute()
                                             .Wait();
        }
        finally
        {
            if (shouldReEnable)
            {
                _logger.LogDebug("Resetting timer to processing logs after 10 seconds");
                _timer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
                _isProcessing = false;
            }
            else
            {
                _timer = null;
            }
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Event Log Queue Collector Service Stopping...");

        _timer?.Change(Timeout.Infinite, Timeout.Infinite);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}