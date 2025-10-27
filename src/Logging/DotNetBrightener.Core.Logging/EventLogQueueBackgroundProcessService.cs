#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.Logging;

public class EventLogQueueBackgroundProcessService(
    ILogger<EventLogQueueBackgroundProcessService> logger,
    IServiceScopeFactory                           serviceScopeFactory)
    : IHostedService, IDisposable
{
    private          Timer?                                         _timer;
    private          bool                                           _isProcessing;

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Starting Event Log Queue Collector Service.");

        _timer = new Timer(DoWork,
                           serviceScopeFactory,
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

        logger.LogDebug("Prevent logger from executing until finish this execution");
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
                logger.LogWarning("QueueEventLogBackgroundProcessing is not registered in the service provider.");
                shouldReEnable = false;

                return;
            }

            queueEventLogBackgroundProcessing.Execute()
                                             .Wait();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred executing Event Log Queue Collector Service.");
        }
        finally
        {
            if (shouldReEnable)
            {
                logger.LogDebug("Resetting timer to processing logs after 10 seconds");
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
        logger.LogDebug("Event Log Queue Collector Service Stopping...");

        _timer?.Change(Timeout.Infinite, Timeout.Infinite);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}