# DotNetBrightener Background Tasks

## Overview

The DotNetBrightener Background Tasks module provides a comprehensive, cron-based task scheduling system for .NET applications. It enables developers to schedule and execute background tasks with precise timing control, overlap prevention, and robust error handling.

## Key Features

- **Flexible Scheduling**: Support for cron expressions, predefined intervals, and one-time execution
- **Overlap Prevention**: Built-in locking mechanism to prevent concurrent execution of the same task
- **Multiple Task Types**: Support for both interface-based tasks (`IBackgroundTask`) and method-based tasks
- **Event-Driven Architecture**: Integration with EventPubSub system for task lifecycle events
- **Database Persistence**: Optional database storage for task definitions and execution history
- **Timezone Support**: Execute tasks in specific timezones
- **Conditional Execution**: Execute tasks based on custom predicates
- **Dependency Injection**: Full integration with .NET's dependency injection container
- **Comprehensive Logging**: Detailed logging for monitoring and debugging

## Architecture

### Core Components

#### IScheduler
The main interface for scheduling and managing background tasks. Provides methods to:
- Schedule tasks by type or method
- Execute tasks at specific times
- Cancel running tasks
- Unschedule tasks

#### IBackgroundTask
Interface that background task classes must implement:
```csharp
public interface IBackgroundTask
{
    Task Execute();
}
```

#### ICancellableTask
Extended interface for tasks that support cancellation:
```csharp
public interface ICancellableTask : IBackgroundTask
{
    CancellationToken CancellationToken { get; set; }
}
```

#### IScheduleConfig
Fluent interface for configuring task schedules with methods like:
- `EverySecond()`, `EveryMinute()`, `Hourly()`, `Daily()`
- `Cron(string expression)`
- `PreventOverlapping()`
- `When(Func<Task<bool>> predicate)`
- `AtTimeZone(TimeZoneInfo timeZoneInfo)`

### Task Execution Flow

1. **SchedulerHostedService** runs every second
2. **Scheduler** checks all registered tasks for due execution
3. Tasks are executed in parallel with proper scoping
4. Events are published for task lifecycle (Started, Ended, Failed)
5. Overlap prevention is enforced if configured
6. Results and errors are logged

## Getting Started

### 1. Installation and Setup

Add the background tasks services to your application:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Enable background task services
builder.Services.EnableBackgroundTaskServices(builder.Configuration);

var app = builder.Build();
```

### 2. Creating Background Tasks

#### Interface-Based Tasks

```csharp
public class EmailCleanupTask : IBackgroundTask
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailCleanupTask> _logger;

    public EmailCleanupTask(IEmailService emailService, ILogger<EmailCleanupTask> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Execute()
    {
        _logger.LogInformation("Starting email cleanup task");
        await _emailService.DeleteOldEmails();
        _logger.LogInformation("Email cleanup task completed");
    }
}
```

#### Cancellable Tasks

```csharp
public class DataProcessingTask : ICancellableTask
{
    public CancellationToken CancellationToken { get; set; }
    
    public async Task Execute()
    {
        while (!CancellationToken.IsCancellationRequested)
        {
            // Process data
            await ProcessBatch();
            await Task.Delay(1000, CancellationToken);
        }
    }
}
```

### 3. Registering Tasks

```csharp
// Register background tasks
builder.Services.AddBackgroundTask<EmailCleanupTask>();
builder.Services.AddBackgroundTask<DataProcessingTask>();
```

### 4. Scheduling Tasks

```csharp
var scheduler = app.Services.GetService<IScheduler>();

// Schedule with predefined intervals
scheduler.ScheduleTask<EmailCleanupTask>()
         .Daily()
         .PreventOverlapping();

// Schedule with custom intervals
scheduler.ScheduleTask<DataProcessingTask>()
         .EverySeconds(30)
         .PreventOverlapping();

// Schedule with cron expressions
scheduler.ScheduleTask<EmailCleanupTask>()
         .Cron("0 2 * * *") // Daily at 2 AM
         .AtTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

// One-time execution
scheduler.ScheduleTask<DataProcessingTask>()
         .Once();
```

## Advanced Features

### Method-Based Scheduling

You can schedule methods directly without implementing `IBackgroundTask`:

```csharp
public class UtilityService
{
    public async Task CleanupTempFiles()
    {
        // Cleanup logic
    }
    
    public void GenerateReports()
    {
        // Report generation logic
    }
}

// Schedule methods
var methodInfo = typeof(UtilityService).GetMethod(nameof(UtilityService.CleanupTempFiles));
scheduler.ScheduleTask(methodInfo)
         .Hourly()
         .PreventOverlapping();
```

### Conditional Execution

Execute tasks only when certain conditions are met:

```csharp
scheduler.ScheduleTask<BackupTask>()
         .Daily()
         .When(async () => await IsMaintenanceWindowOpen())
         .PreventOverlapping();
```

### Timezone-Aware Scheduling

```csharp
scheduler.ScheduleTask<ReportTask>()
         .DailyAt(9, 0) // 9:00 AM
         .AtTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
```

## Configuration

### Basic Configuration

Configure the scheduler interval in `appsettings.json`:

```json
{
  "BackgroundTaskOptions": {
    "Interval": "00:00:30"
  }
}
```

### Database Storage (Optional)

For persistent task definitions and execution history:

```csharp
builder.Services.AddBackgroundTaskStorage(options =>
{
    options.UseSqlServer(connectionString);
});
```

This creates a `BackgroundTaskDefinition` table to store:
- Task assembly and type information
- Cron expressions and timezone settings
- Execution history and error logs
- Enable/disable status

## Event System Integration

The background tasks system publishes events through the EventPubSub system:

### Available Events

- `ScheduledEventStarted`: Published when a task begins execution
- `ScheduledEventEnded`: Published when a task completes successfully
- `ScheduledEventFailed`: Published when a task throws an exception

### Event Handlers

```csharp
public class TaskMonitoringHandler : IEventHandler<ScheduledEventFailed>
{
    public async Task<bool> HandleEvent(ScheduledEventFailed eventMessage)
    {
        // Log error, send notifications, etc.
        return true;
    }
}
```

## Scheduling Options Reference

### Predefined Intervals

- `EverySecond()` - Every second
- `EverySeconds(int seconds)` - Every N seconds
- `EveryMinute()` - Every minute
- `EveryFiveMinutes()` - Every 5 minutes
- `EveryTenMinutes()` - Every 10 minutes
- `EveryFifteenMinutes()` - Every 15 minutes
- `EveryThirtyMinutes()` - Every 30 minutes
- `Hourly()` - Every hour
- `HourlyAt(int minute)` - Every hour at specified minute
- `Daily()` - Every day at midnight
- `DailyAt(int hour, int minute)` - Every day at specified time
- `Weekly()` - Every week
- `Monthly()` - Every month

### Day-of-Week Restrictions

- `Monday()`, `Tuesday()`, `Wednesday()`, `Thursday()`, `Friday()`, `Saturday()`, `Sunday()`
- `Weekday()` - Monday through Friday
- `Weekend()` - Saturday and Sunday

### Cron Expressions

Support for standard 5 or 6-part cron expressions:
- 5-part: `minute hour day month weekday`
- 6-part: `second minute hour day month weekday`

Examples:
- `"0 2 * * *"` - Daily at 2:00 AM
- `"*/15 * * * *"` - Every 15 minutes
- `"0 0 * * 0"` - Every Sunday at midnight
- `"30 14 1 * *"` - 2:30 PM on the 1st of every month

## Error Handling and Monitoring

### Logging

The system provides comprehensive logging at various levels:
- Task execution start/end
- Error details with stack traces
- Performance metrics (execution duration)
- Overlap prevention actions

### Exception Handling

- Exceptions in tasks are caught and logged
- Failed tasks don't affect other scheduled tasks
- `ScheduledEventFailed` events are published for monitoring

### Performance Monitoring

- Execution duration tracking
- Concurrent execution monitoring
- Scheduler iteration counts

## Best Practices

1. **Use Dependency Injection**: Register tasks as scoped services for proper resource management
2. **Implement Proper Logging**: Use structured logging for better monitoring
3. **Handle Cancellation**: Implement `ICancellableTask` for long-running tasks
4. **Prevent Overlapping**: Use `PreventOverlapping()` for tasks that shouldn't run concurrently
5. **Use Appropriate Intervals**: Don't over-schedule tasks; consider system resources
6. **Monitor Performance**: Watch execution times and adjust schedules accordingly
7. **Handle Exceptions**: Implement proper error handling within tasks
8. **Use Timezone Awareness**: Specify timezones for business-critical scheduling

## Troubleshooting

### Common Issues

1. **Tasks Not Executing**: Check if `EnableBackgroundTaskServices()` is called
2. **Dependency Resolution Errors**: Ensure tasks are registered with `AddBackgroundTask<T>()`
3. **Overlapping Prevention Not Working**: Verify unique identifiers are properly set
4. **Timezone Issues**: Use IANA timezone identifiers for cross-platform compatibility

### Debugging

Enable detailed logging to troubleshoot issues:

```csharp
builder.Services.AddLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Debug);
    logging.AddConsole();
});
```

## Migration and Deployment

When using database storage, the system automatically handles database migrations through the `MigrateBackgroundTaskDbContextHostedService`.

## Performance Considerations

- The scheduler runs every second by default
- Tasks execute in parallel using separate service scopes
- Overlap prevention uses in-memory locking with configurable timeouts
- Database operations are optimized with proper indexing