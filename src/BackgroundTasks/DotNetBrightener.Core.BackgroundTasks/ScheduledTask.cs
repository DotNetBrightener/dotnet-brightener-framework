using DotNetBrightener.Core.BackgroundTasks.Cron;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace DotNetBrightener.Core.BackgroundTasks;

public class ScheduledTask : IScheduleConfig
{
    public TaskActionDescriptor ScheduledTaskAction { get; private init; }
    public Type                 InvocableType       { get; private init; }

    private readonly IServiceScopeFactory _scopeFactory;

    private CronExpression   _expression;
    private bool             _preventOverlapping;
    private string           _eventUniqueId;
    private Func<Task<bool>> _whenPredicate;
    private TimeZoneInfo     _zonedTime = TimeZoneInfo.Utc;
    private bool             _runOnce;
    private bool             _hasExecuted;

    private ScheduledTask(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory  = scopeFactory;
        _eventUniqueId = Guid.NewGuid().ToString();
    }

    internal static ScheduledTask WithAction(IServiceScopeFactory scopeFactory,
                                             MethodInfo           scheduledAsyncTask,
                                             params object[]      parameters)
    {
        return new ScheduledTask(scopeFactory)
        {
            ScheduledTaskAction = new TaskActionDescriptor(scheduledAsyncTask, parameters)
        };
    }


    internal static ScheduledTask WithInvocable<T>(IServiceScopeFactory scopeFactory) where T : IBackgroundTask
        => WithInvocableType(typeof(T), scopeFactory);

    internal static ScheduledTask WithInvocableType(Type                 invocableType,
                                                    IServiceScopeFactory scopeFactory)
    {
        return new ScheduledTask(scopeFactory)
        {
            InvocableType = invocableType
        };
    }


    internal async Task Execute(ILogger logger, CancellationToken cancellationToken)
    {
        if (await WhenPredicateFails())
        {
            return;
        }

        if (ScheduledTaskAction is not null)
        {
            await ExecuteTaskAction(logger, cancellationToken);
        }
        else
        {
            await ExecuteInvocableTask(logger, cancellationToken);
        }

        MarkedAsExecuted();
        UnscheduleIfNeeded();
    }

    private async Task ExecuteInvocableTask(ILogger logger, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        logger.LogInformation("Starting new service scope...");

        var taskName = InvocableType.FullName;

        await using AsyncServiceScope serviceScope = new(_scopeFactory.CreateAsyncScope());

        logger.LogInformation("Resolving task instance of type {taskName}...", taskName);

        if (serviceScope.ServiceProvider.GetRequiredService(InvocableType) is not IBackgroundTask invocable)
        {
            logger.LogWarning("Cannot resolve task instance of type {taskName}. Exiting...", taskName);

            return;
        }

        if (invocable is ICancellableTask cancellableInvokable)
        {
            cancellableInvokable.CancellationToken = cancellationToken;
        }

        try
        {
            logger.LogInformation("Executing task {taskName}...", taskName);
            await invocable.Execute();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while running background task {taskName}", taskName);
        }
        finally
        {
            stopWatch.Stop();
            logger.LogInformation("Finished executing task {taskName} in {elapsed}",
                                  invocable.GetType().FullName,
                                  stopWatch.Elapsed);
        }
    }

    private async Task ExecuteTaskAction(ILogger logger, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();
        logger.LogInformation("Starting new service scope...");

        await using AsyncServiceScope scope = new(_scopeFactory.CreateAsyncScope());

        var backgroundServiceProvider = scope.ServiceProvider;
        var invocableType             = ScheduledTaskAction.InvocableType;

        object invokingInstance = null;

        try
        {
            logger.LogInformation("Trying to get instance of {invocableType}...", invocableType);

            invokingInstance = backgroundServiceProvider.TryGet(invocableType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while resolving instance of {invocableType}", invocableType);
        }

        if (invokingInstance is null)
        {
            logger.LogError("Unable to execute background queued task. " +
                            "Could not find the {backgroundTaskType} instance that can invoke the scheduled method",
                            invocableType.Name);

            return;
        }

        try
        {
            await ScheduledTaskAction.Invoke(logger, invokingInstance, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                            "Error while executing background task {backgroundTaskType}.",
                            invocableType.Name);
        }
        finally
        {
            stopWatch.Stop();
            logger.LogInformation("Finished executing task {taskName} in {elapsed}",
                                  ScheduledTaskAction.TaskName,
                                  stopWatch.Elapsed);
        }
    }

    private void MarkedAsExecuted()
    {
        _hasExecuted = true;
    }

    private void UnscheduleIfNeeded()
    {
        if (_runOnce && _hasExecuted)
        {
            using var scope     = _scopeFactory.CreateScope();
            var       scheduler = scope.ServiceProvider.GetService<IScheduler>();
            scheduler?.TryUnscheduleTask(_eventUniqueId);
        }
    }

    public bool IsDue(DateTime utcNow)
    {
        var zonedNow = TimeZoneInfo.ConvertTime(utcNow, _zonedTime);

        var isDue = _expression?.IsDue(zonedNow) ?? false;  

        return isDue;
    }

    public IScheduleConfig PreventOverlapping(string uniqueIdentifier = null)
    {
        _preventOverlapping = true;

        if (!string.IsNullOrEmpty(uniqueIdentifier))
            _eventUniqueId = uniqueIdentifier;

        return this;
    }

    public string OverlappingUniqueIdentifier() => _eventUniqueId;

    public IScheduleConfig Daily()
    {
        _expression = new CronExpression("00 00 * * *");

        return this;
    }

    public IScheduleConfig DailyAtHour(int hour)
    {
        _expression = new CronExpression($"00 {hour:00} * * *");

        return this;
    }

    public IScheduleConfig DailyAt(int hour, int minute)
    {
        _expression = new CronExpression($"{minute:00} {hour:00} * * *");

        return this;
    }

    public IScheduleConfig Hourly()
    {
        _expression = new CronExpression("00 * * * *");

        return this;
    }

    public IScheduleConfig HourlyAt(int minute)
    {
        _expression = new CronExpression($"{minute:00} * * * *");

        return this;
    }

    public IScheduleConfig EveryMinute()
    {
        _expression = new CronExpression("* * * * *");

        return this;
    }

    public IScheduleConfig EveryFiveMinutes()
    {
        _expression = new CronExpression("*/5 * * * *");

        return this;
    }

    public IScheduleConfig EveryTenMinutes()
    {
        _expression = new CronExpression("*/10 * * * *");

        return this;
    }

    public IScheduleConfig EveryFifteenMinutes()
    {
        _expression = new CronExpression("*/15 * * * *");

        return this;
    }

    public IScheduleConfig EveryThirtyMinutes()
    {
        _expression = new CronExpression("*/30 * * * *");

        return this;
    }

    public IScheduleConfig Weekly()
    {
        _expression = new CronExpression("00 00 * * 1");

        return this;
    }

    public IScheduleConfig Monthly()
    {
        _expression = new CronExpression("00 00 1 * *");

        return this;
    }

    public IScheduleConfig Cron(string cronExpression)
    {
        _expression = new CronExpression(cronExpression);

        return this;
    }

    public IScheduleConfig Monday()
    {
        _expression.AppendWeekDay(DayOfWeek.Monday);

        return this;
    }

    public IScheduleConfig Tuesday()
    {
        _expression.AppendWeekDay(DayOfWeek.Tuesday);

        return this;
    }

    public IScheduleConfig Wednesday()
    {
        _expression.AppendWeekDay(DayOfWeek.Wednesday);

        return this;
    }

    public IScheduleConfig Thursday()
    {
        _expression.AppendWeekDay(DayOfWeek.Thursday);

        return this;
    }

    public IScheduleConfig Friday()
    {
        _expression.AppendWeekDay(DayOfWeek.Friday);

        return this;
    }

    public IScheduleConfig Saturday()
    {
        _expression.AppendWeekDay(DayOfWeek.Saturday);

        return this;
    }

    public IScheduleConfig Sunday()
    {
        _expression.AppendWeekDay(DayOfWeek.Sunday);

        return this;
    }

    public IScheduleConfig Weekday()
    {
        Monday()
           .Tuesday()
           .Wednesday()
           .Thursday()
           .Friday();

        return this;
    }

    public IScheduleConfig Weekend()
    {
        Saturday().Sunday();

        return this;
    }

    public IScheduleConfig When(Func<Task<bool>> predicate)
    {
        _whenPredicate = predicate;

        return this;
    }

    public IScheduleConfig AtTimeZone(TimeZoneInfo timeZoneInfo)
    {
        _zonedTime = timeZoneInfo;

        return this;
    }

    public IScheduleConfig Once()
    {
        _runOnce = true;

        return this;
    }

    public IScheduleConfig EverySecond()
    {
        return EverySeconds(1);
    }

    public IScheduleConfig EveryFiveSeconds()
    {
        return EverySeconds(5);
    }

    public IScheduleConfig EveryTenSeconds()
    {
        return EverySeconds(10);
    }

    public IScheduleConfig EveryFifteenSeconds()
    {
        return EverySeconds(15);
    }

    public IScheduleConfig EveryThirtySeconds()
    {
        return EverySeconds(30);
    }

    public IScheduleConfig EverySeconds(int seconds)
    {
        if (seconds < 1 ||
            seconds > 59)
        {
            throw new ArgumentException(
                                        "When calling 'EverySeconds(int seconds)', 'seconds' must be between 0 and 60");
        }

        var secondPart = seconds == 1 ? "*" : $"*/{seconds}";

        _expression = new CronExpression($"{secondPart} * * * * *");

        return this;
    }

    internal bool ShouldRunOnce() => _runOnce && !_hasExecuted;

    public bool ShouldPreventOverlapping() => _preventOverlapping;

    private async Task<bool> WhenPredicateFails()
    {
        return _whenPredicate != null && !await _whenPredicate.Invoke();
    }
}