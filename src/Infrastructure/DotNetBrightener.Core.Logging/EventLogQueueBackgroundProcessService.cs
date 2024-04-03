#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Core.Logging;

public class EventLogQueueBackgroundProcessService : IHostedService, IDisposable
{
    private readonly ILogger<EventLogQueueBackgroundProcessService> _logger;
    private readonly IServiceScopeFactory                           _serviceScopeFactory;
    private          Timer?                                         _timer = null;

    public EventLogQueueBackgroundProcessService(ILogger<EventLogQueueBackgroundProcessService> logger,
                                                 IServiceScopeFactory                           serviceScopeFactory)
    {
        _logger              = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Event Log Queue Collector Service.");

        _timer = new Timer(DoWork,
                           _serviceScopeFactory,
                           TimeSpan.Zero,
                           TimeSpan.FromSeconds(10));

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        if (state is IServiceScopeFactory serviceScopeFactory)
        {
            using var scope = serviceScopeFactory.CreateScope();

            var queueEventLogBackgroundProcessing = scope.ServiceProvider.GetRequiredService<IQueueEventLogBackgroundProcessing>();

            queueEventLogBackgroundProcessing.Execute();
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event Log Queue Collector Service Stopping...");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}