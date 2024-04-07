using DotNetBrightener.Core.BackgroundTasks.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.Core.BackgroundTasks.HostedServices;

/// <summary>
///     The host service that runs the background tasks in a fixed interval.
/// </summary>
internal class BackgroundTaskHostService : IHostedService, IDisposable
{
    private readonly ILogger<BackgroundTaskHostService> _logger;
    private readonly IHostApplicationLifetime           _lifetime;
    private readonly IServiceScopeFactory               _serviceScopeFactory;

    private Timer _timer;
    private bool  _schedulerEnabled = true;

    public BackgroundTaskHostService(ILogger<BackgroundTaskHostService> logger,
                                     IHostApplicationLifetime           lifetime,
                                     IServiceScopeFactory               serviceScopeFactory)
    {
        _logger              = logger;
        _lifetime            = lifetime;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(InitializeAfterAppStarted);

        return Task.CompletedTask;
    }

    private void InitializeAfterAppStarted()
    {
        _timer = new Timer(ExecuteTask, _serviceScopeFactory, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private async void ExecuteTask(object state)
    {
        // if the scheduler is told to stop, we should not run any more tasks.
        if (!_schedulerEnabled ||
            state is not IServiceScopeFactory serviceScopeFactory)
            return;

        _logger.LogInformation("Temporarily disable timer while background tasks are executing...");
        _schedulerEnabled = false;
        _timer?.Change(Timeout.Infinite, 0);

        using var scope = serviceScopeFactory.CreateScope();

        try
        {
            var manager = scope.ServiceProvider.GetRequiredService<IBackgroundTaskRunner>();

            await manager.DoWork();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing background tasks.");
        }

        var backgroundTaskOptions = scope.ServiceProvider.GetRequiredService<IOptions<BackgroundTaskOptions>>();

        // reenable timer so it triggers again
        _schedulerEnabled = true;
        var timerInterval = backgroundTaskOptions.Value.Interval;

        _timer?.Change(timerInterval, timerInterval);
        _logger.LogInformation("Timer re-enabled with interval {timerInterval}.", timerInterval);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // stop the scheduler so there will be no more task 
        _schedulerEnabled = false;

        _timer?.Change(Timeout.Infinite, 0);
    }

    public void Dispose()
    {
        _timer?.Dispose();

        _logger.LogInformation("Background Task Host Service is now stopped.");
    }
}