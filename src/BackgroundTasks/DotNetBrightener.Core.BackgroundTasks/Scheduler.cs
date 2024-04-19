using DotNetBrightener.Core.BackgroundTasks.Event;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace DotNetBrightener.Core.BackgroundTasks;

public class Scheduler : IScheduler
{
    private readonly ConcurrentDictionary<string, ScheduledTask> _tasks = new();
    private readonly IServiceScopeFactory                        _scopeFactory;
    private readonly ILockedTasksContainer                       _lockedTasksContainer;
    private readonly ILogger                                     _logger;
    private readonly CancellationTokenSource                     _cancellationTokenSource;

    private int  _schedulerIterationsActiveCount;

    public bool IsRunning => _schedulerIterationsActiveCount > 0;

    public Scheduler(IServiceScopeFactory  scopeFactory,
                     ILockedTasksContainer lockedTasksContainer,
                     ILogger<Scheduler>    logger)
    {
        _scopeFactory            = scopeFactory;
        _logger                  = logger;
        _lockedTasksContainer    = lockedTasksContainer;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public IScheduleConfig ScheduleTask(MethodInfo methodAction, params object[] parameters)
    {
        var scheduled = ScheduledTask.WithAction(_scopeFactory, methodAction, parameters);

        _tasks.TryAdd(scheduled.OverlappingUniqueIdentifier(), scheduled);

        return scheduled;
    }

    public IScheduleConfig ScheduleTask<T>()
        where T : IBackgroundTask
    {
        var scheduled = ScheduledTask.WithInvocable<T>(_scopeFactory);

        _tasks.TryAdd(scheduled.OverlappingUniqueIdentifier(), scheduled);

        return scheduled;
    }

    public IScheduleConfig ScheduleTask(Type taskType)
    {
        var scheduled = ScheduledTask.WithInvocableType(taskType, _scopeFactory);

        _tasks.TryAdd(scheduled.OverlappingUniqueIdentifier(), scheduled);

        return scheduled;
    }

    public async Task RunAt(DateTime tick)
    {
        Interlocked.Increment(ref _schedulerIterationsActiveCount);

        await ExecuteWorker(tick);

        Interlocked.Decrement(ref _schedulerIterationsActiveCount);
    }

    private async Task ExecuteWorker(DateTime tick)
    {
        var scheduledWorkers = new List<ScheduledTask>();

        foreach (var keyValue in _tasks)
        {
            if (keyValue.Value.ShouldRunOnce() ||
                keyValue.Value.IsDue(tick))
            {
                scheduledWorkers.Add(keyValue.Value);
            }
        }

        var tasks = scheduledWorkers.Select(InvokeEventWithLoggerScope);

        await Task.WhenAll(tasks);
    }

    private async Task InvokeEventWithLoggerScope(ScheduledTask scheduledEvent)
    {

        if (scheduledEvent.InvocableType is null)
        {
            await InvokeEvent(scheduledEvent);

            return;
        }

        var eventInvocableTypeName = scheduledEvent.InvocableType?.Name;

        using (_logger.BeginScope($"BackgroundTask [{eventInvocableTypeName}]"))
        {
            await InvokeEvent(scheduledEvent);
        }
    }

    private async Task InvokeEvent(ScheduledTask scheduledTask)
    {
        await using var scope          = _scopeFactory.CreateAsyncScope();
        var             scopeProvider  = scope.ServiceProvider;
        var             eventPublisher = scopeProvider.GetService<IEventPublisher>();


        async Task Invoke()
        {
            _logger.LogDebug("Scheduled task started...");

            await scheduledTask.Execute(_logger, _cancellationTokenSource.Token);

            _logger.LogDebug("Scheduled task finished...");
        }

        try
        {
            await eventPublisher.Publish(new ScheduledEventStarted(scheduledTask));

            if (scheduledTask.ShouldPreventOverlapping())
            {
                if (_lockedTasksContainer.TryLock(scheduledTask.OverlappingUniqueIdentifier(),
                                                  TimeSpan.FromHours(24)))
                {
                    try
                    {
                        await Invoke();
                    }
                    finally
                    {
                        _lockedTasksContainer.Release(scheduledTask.OverlappingUniqueIdentifier());
                    }
                }
            }
            else
            {
                await Invoke();
            }

            await eventPublisher.Publish(new ScheduledEventEnded(scheduledTask));
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "A scheduled task threw an Exception: ");

            await eventPublisher.Publish(new ScheduledEventFailed(scheduledTask, e));
        }
    }

    public async Task CancelAllCancellableTasks()
    {
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            await _cancellationTokenSource.CancelAsync();
        }
    }

    public bool TryUnscheduleTask(string uniqueIdentifier)
    {
        _logger.LogInformation("Unscheduling task with unique identifier: {uniqueIdentifier}", uniqueIdentifier);

        var toUnSchedule = _tasks.FirstOrDefault(scheduledEvent => scheduledEvent.Value
                                                                                 .OverlappingUniqueIdentifier() ==
                                                                   uniqueIdentifier);

        if (toUnSchedule.Value != null)
        {
            var guid = toUnSchedule.Key;

            if (_tasks.TryRemove(guid, out var task))
            {
                _logger.LogInformation("Task with unique identifier {uniqueIdentifier}, of type {invocableType} has been unscheduled.",
                                       uniqueIdentifier,
                                       task?.InvocableType?.Name);
            }
            else
            {
                _logger.LogInformation("Failed to unschedule task with unique identifier {uniqueIdentifier}, of type {invocableType}.",
                                       uniqueIdentifier,
                                       task?.InvocableType?.Name);
            }
        }

        return true;
    }
}