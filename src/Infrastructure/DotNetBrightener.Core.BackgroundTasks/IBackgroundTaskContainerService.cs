using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace DotNetBrightener.Core.BackgroundTasks;

/// <summary>
/// Represents the service that hosts all background tasks available in system.
/// </summary>
public interface IBackgroundTaskContainerService
{
    TimeSpan Interval { get; set; }

    void Activate();

    void Terminate();
}

public class BackgroundTaskContainerService : IBackgroundTaskContainerService, IDisposable
{
    private readonly IServiceProvider _serviceResolver;
    private readonly Timer            _timer;
    private readonly ILogger          _logger;

    public BackgroundTaskContainerService(ILogger<BackgroundTaskContainerService> logger,
                                          IBackgroundServiceProvider              backgroundServiceProvider)
    {
        _serviceResolver = backgroundServiceProvider;
        _logger          = logger;
        _timer           = new Timer();

        Interval = TimeSpan.FromSeconds(30);

        _timer.Elapsed += Elapsed;
    }

    public TimeSpan Interval
    {
        get => TimeSpan.FromMilliseconds(_timer.Interval);
        set => _timer.Interval = value.TotalMilliseconds;
    }

    public void Activate()
    {
        lock (_timer)
        {
            _timer.Start();
        }
    }

    public void Terminate()
    {
        lock (_timer)
        {
            _timer.Stop();
        }
    }

    void Elapsed(object sender, ElapsedEventArgs e)
    {
        // current implementation disallows re-entrancy
        if (!Monitor.TryEnter(_timer))
            return;

        try
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
                DoWork().Wait();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error while executing background task");
        }
        finally
        {
            _timer.Start();
            Monitor.Exit(_timer);
        }
    }

    private Task DoWork()
    {
        return Task.Run(async () =>
        {
            using var backgroundScope = _serviceResolver.CreateScope();
            var       manager         = backgroundScope.ServiceProvider.GetRequiredService<IBackgroundTaskRunner>();
            await manager.DoWork();
        });
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}