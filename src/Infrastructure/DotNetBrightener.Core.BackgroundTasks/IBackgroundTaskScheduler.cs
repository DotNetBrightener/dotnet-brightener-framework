using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DotNetBrightener.Core.BackgroundTasks;

/// <summary>
///     Represents a scheduler to run background tasks
/// </summary>
public interface IBackgroundTaskScheduler
{
    string EnqueueTask(MethodInfo methodAction, params object[] parameters);

    QueuedTaskResult GetTaskProcessResult(string taskIdentifier);

    void RemoveTaskResult(string taskIdentifier);
}

public class BackgroundTaskScheduler : IBackgroundTaskScheduler, IDisposable
{
    private readonly Queue<BackgroundQueuedTask>                        _tasks        = new();
    private readonly ConcurrentDictionary<string, BackgroundQueuedTask> _runningTasks = new();
    private readonly IServiceScopeFactory                               _serviceResolver;
    private readonly Timer                                              _timer;
    private readonly ILogger                                            _logger;
    private          bool                                               _isActivated;

    public BackgroundTaskScheduler(ILogger<BackgroundTaskScheduler> logger,
                                   IServiceScopeFactory             backgroundServiceProvider)
    {
        _serviceResolver =  backgroundServiceProvider;
        _logger          =  logger;
        _timer           =  new Timer();
        _timer.Interval  =  TimeSpan.FromSeconds(3).TotalMilliseconds;
        _timer.Elapsed   += Elapsed;
    }

    public void Activate()
    {
        lock (_timer)
        {
            _logger.LogInformation("Activating background task scheduler...");
            _timer.Start();
            _isActivated = true;
        }
    }

    public string EnqueueTask(MethodInfo methodAction, params object[] parameters)
    {
        var queuedTask = new BackgroundQueuedTask
        {
            Action     = methodAction,
            Parameters = parameters
        };

        _logger.LogInformation("Adding task {task} to queue...", queuedTask.TaskName);

        _tasks.Enqueue(queuedTask);

        if (!_timer.Enabled ||  !_isActivated)
        {
            Activate();
        }

        return queuedTask.TaskIdentifier;
    }

    public QueuedTaskResult GetTaskProcessResult(string taskIdentifier)
    {
        var queuedTask = _tasks.FirstOrDefault(_ => _.TaskIdentifier == taskIdentifier);

        if (queuedTask == null)
        {
            _runningTasks.TryGetValue(taskIdentifier, out queuedTask);
        }

        if (queuedTask == null)
            return null;

        return new QueuedTaskResult
        {
            TaskIdentifier = taskIdentifier,
            Started        = queuedTask.StartedOn,
            TaskResult     = queuedTask.TaskResult
        };
    }

    public void RemoveTaskResult(string taskIdentifier)
    {
        if (_runningTasks.TryGetValue(taskIdentifier, out _))
        {
            _runningTasks.TryRemove(taskIdentifier, out _);
        }
    }

    void Elapsed(object sender, ElapsedEventArgs e)
    {
        // current implementation disallows re-entrancy
        if (!Monitor.TryEnter(_timer))
        {
            _logger.LogDebug("Timer is being locked for another execution. Exiting...");

            return;
        }

        Stopwatch sw = null;

        try
        {
            _logger.LogDebug("Stop and lock timer for execution.");
            _timer.Stop();
            sw = Stopwatch.StartNew();
            DoWork();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error while executing background task");
        }
        finally
        {
            if (_tasks.Count > 0)
            {
                _logger.LogInformation("Restarting timer for next execution.");
                _timer.Start();
            }
            else
            {
                _logger.LogDebug("No tasks to execute. Exiting...");
            }

            Monitor.Exit(_timer);

            sw!.Stop();
            _logger.LogInformation("Background task execution finished after {elapsed}.", sw.Elapsed);
        }
    }

    private void DoWork()
    {
        if (_tasks.Count == 0)
        {
            _logger.LogInformation("No tasks to execute. Exiting...");

            return;
        }

        var tasksList = new List<Task>();

        while (_tasks.Any())
        {
            var taskToRun = _tasks.Dequeue();

            if (taskToRun != null)
            {
                tasksList.Add(RunTask(taskToRun));
            }
        }

        Task.WaitAll(tasksList.ToArray());
    }

    private async Task RunTask(BackgroundQueuedTask taskToRun)
    {
        taskToRun.StartedOn = DateTimeOffset.UtcNow;
        _runningTasks.TryAdd(taskToRun.TaskIdentifier, taskToRun);

        using var backgroundScope = _serviceResolver.CreateScope();

        var backgroundServiceProvider = backgroundScope.ServiceProvider;

        var declaringType = taskToRun.Action.DeclaringType;

        if (declaringType == null)
            return;

        object invokingInstance = null;

        try
        {
            _logger.LogInformation("Trying to get instance of {declaringType}...", declaringType);
            invokingInstance = backgroundServiceProvider.TryGet(declaringType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while resolving instance of {declaringType}", declaringType);
        }

        if (invokingInstance is null)
        {
            _logger.LogError("Unable to execute background queued task. " +
                             "Could not find the {declaringTypeFullName} instance that can invoke the scheduled method",
                             declaringType.FullName);

            return;
        }

        var isAwaitable = taskToRun.Action.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;

        Task ExecuteMethod()
        {
            _logger.LogInformation("Executing task: {taskName} ({awaitable})",
                                   taskToRun.TaskName,
                                   isAwaitable ? "awaitable" : "non-awaitable");

            if (isAwaitable)
            {
                var invokeResult = Task.Run(async () =>
                {
                    try
                    {
                        var result = await (dynamic)taskToRun.Action.Invoke(invokingInstance, taskToRun.Parameters);

                        return result;
                    }
                    catch
                    {
                        return null;
                    }
                });

                taskToRun.TaskResult = invokeResult;

                return taskToRun.TaskResult;
            }

            taskToRun.Action.Invoke(invokingInstance, taskToRun.Parameters);

            return Task.CompletedTask;
        }

        var sw = Stopwatch.StartNew();

        try
        {
            await ExecuteMethod();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                             "Error occurred when executing background queued task {taskName}",
                             taskToRun.TaskName);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("Task {taskName} finished in {elapsed}",
                                   taskToRun.TaskName,
                                   sw.Elapsed);
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}