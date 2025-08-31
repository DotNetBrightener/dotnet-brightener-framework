# DotNetBrightener Activity Logging Module

A comprehensive activity logging module with Aspect-Oriented Programming (AOP) capabilities for .NET applications. This module provides automatic method execution logging with performance monitoring, exception tracking, and configurable serialization using a clean repository pattern architecture.

## Features

- **Automatic Method Logging**: Use the `[LogActivity]` attribute to automatically log method execution
- **High-Performance Interception**: Built on Castle DynamicProxy for minimal overhead
- **Repository Pattern Architecture**: Clean separation of concerns with pluggable storage providers
- **Async Support**: Full support for async/await methods including Task, Task<T>, ValueTask, and ValueTask<T>
- **Performance Monitoring**: High-precision timing with configurable slow method detection
- **Exception Handling**: Comprehensive exception capture with full stack traces and inner exceptions
- **Configurable Serialization**: Smart serialization with depth control and sensitive data filtering
- **Async Logging Pipeline**: Background processing with batching for optimal performance
- **Multiple Database Providers**: Built-in support for SQL Server, PostgreSQL, and in-memory testing
- **Correlation Tracking**: Automatic correlation ID generation for tracking related activities
- **Flexible Configuration**: Extensive configuration options with fluent builder API

## Architecture

The Activity Logging Module follows a clean architecture pattern with separate concerns:

### Core Projects

- **`ActivityLog`**: Core business logic, interfaces, and configuration
- **`ActivityLog.DataStorage`**: Repository implementation and base DbContext
- **`ActivityLog.DataStorage.SqlServer`**: SQL Server provider with migrations
- **`ActivityLog.DataStorage.PostgreSql`**: PostgreSQL provider with migrations

### Repository Pattern

The module uses the DotNetBrightener framework's repository pattern:

```csharp
public interface IActivityLogRepository : IRepository
{
    // Inherits standard repository methods from DotNetBrightener framework
}
```

## Quick Start

### 1. Installation

```bash
# Core module
dotnet add package DotNetBrightener.ActivityLog

# SQL Server provider (optional)
dotnet add package DotNetBrightener.ActivityLog.DataStorage.SqlServer

# PostgreSQL provider (optional)
dotnet add package DotNetBrightener.ActivityLog.DataStorage.PostgreSql
```

### 2. Basic Setup (No Persistence)

```csharp
// Program.cs - Basic setup without database persistence
services.AddActivityLogging(configuration.GetSection("ActivityLogging"));
```

### 3. Setup with Database Persistence

```csharp
// SQL Server
services.AddActivityLogging(configuration.GetSection("ActivityLogging"))
        .WithStorage()
        .UseSqlServer(connectionString);

// PostgreSQL
services.AddActivityLogging(configuration.GetSection("ActivityLogging"))
        .WithStorage()
        .UsePostgreSql(connectionString);

// In-Memory (for testing)
services.AddActivityLogging(configuration.GetSection("ActivityLogging"))
        .WithStorage()
        .UseInMemoryDatabase("TestDb");
```

### 4. Configuration

```json
{
  "ActivityLogging": {
    "IsEnabled": true,
    "MinimumLogLevel": "Information",
    "Serialization": {
      "MaxDepth": 3,
      "MaxStringLength": 1000,
      "SerializeInputParameters": true,
      "SerializeReturnValues": true,
      "ExcludedProperties": ["Password", "Secret", "Token"],
      "ExcludedTypes": ["System.IO.Stream", "Microsoft.AspNetCore.Http.HttpContext"]
    },
    "Performance": {
      "EnableHighPrecisionTiming": true,
      "SlowMethodThresholdMs": 1000,
      "LogOnlySlowMethods": false
    },
    "AsyncLogging": {
      "EnableAsyncLogging": true,
      "BatchSize": 100,
      "FlushIntervalMs": 5000,
      "MaxQueueSize": 10000
    },
    "ExceptionHandling": {
      "CaptureFullStackTrace": true,
      "CaptureInnerExceptions": true,
      "ContinueOnLoggingFailure": true
    }
  }
}
```

## Usage Examples

### Basic Method Logging

```csharp
public interface IUserService
{
    Task<User> GetUserAsync(int userId);
    Task<User> CreateUserAsync(CreateUserRequest request);
}

public class UserService : IUserService
{
    [LogActivity("GetUser", "Retrieved user with ID: {0}")]
    public async Task<User> GetUserAsync(int userId)
    {
        // Method implementation
        return await _repository.GetByIdAsync(userId);
    }

    [LogActivity("CreateUser", "Created new user: {0}")]
    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Method implementation
        var user = new User { Name = request.Name, Email = request.Email };
        return await _repository.CreateAsync(user);
    }
}
```

### Service Registration

```csharp
// Register specific services with activity logging
services.AddActivityLoggedService<IUserService, UserService>();
services.AddActivityLoggedService<IOrderService, OrderService>(ServiceLifetime.Singleton);
```

## Data Model

### ActivityLogRecord Entity

The `ActivityLogRecord` entity captures comprehensive method execution information:

```csharp
public class ActivityLogRecord
{
    public Guid Id { get; set; }                    // Primary key (Version 7 GUID)
    public string ActivityName { get; set; }        // Method or activity name
    public string? ActivityDescription { get; set; } // Detailed description

    // User Context
    public long? UserId { get; set; }               // User identifier
    public string? UserName { get; set; }           // User name

    // Target Entity (optional)
    public string? TargetEntity { get; set; }       // Entity being operated on
    public string? TargetEntityId { get; set; }     // Entity identifier

    // Timing Information
    public DateTimeOffset StartTime { get; set; }   // Method start time (high precision)
    public DateTimeOffset? EndTime { get; set; }    // Method end time
    public double? ExecutionDurationMs { get; set; } // Duration in milliseconds

    // Method Information
    public string? MethodName { get; set; }         // Full method name with class
    public string? ClassName { get; set; }          // Class name
    public string? Namespace { get; set; }          // Namespace

    // Execution Data
    public string? InputParameters { get; set; }    // Serialized input parameters
    public string? ReturnValue { get; set; }        // Serialized return value

    // Exception Information
    public string? Exception { get; set; }          // Full exception details
    public string? ExceptionType { get; set; }      // Exception type name
    public bool IsSuccess { get; set; }             // Success indicator

    // Context Information
    public string? Metadata { get; set; }           // Additional metadata
    public string? UserAgent { get; set; }          // HTTP User-Agent
    public string? IpAddress { get; set; }          // Client IP address
    public Guid? CorrelationId { get; set; }        // Correlation tracking
    public string? LogLevel { get; set; }           // Log level
    public string? Tags { get; set; }               // Categorization tags
}
```

## Configuration Options

### Core Configuration

```csharp
services.ConfigureActivityLogging(options =>
{
    options.IsEnabled = true;
    options.MinimumLogLevel = ActivityLogLevel.Information;

    // Performance settings
    options.Performance.EnableHighPrecisionTiming = true;
    options.Performance.SlowMethodThresholdMs = 1000;
    options.Performance.LogOnlySlowMethods = false;

    // Serialization settings
    options.Serialization.MaxDepth = 3;
    options.Serialization.MaxStringLength = 1000;
    options.Serialization.SerializeInputParameters = true;
    options.Serialization.SerializeReturnValues = true;
    options.Serialization.ExcludedProperties.Add("Password");
    options.Serialization.ExcludedTypes.Add("System.IO.Stream");

    // Filtering settings
    options.Filtering.ExcludedNamespaces.Add("System");
    options.Filtering.ExcludedMethods.Add("ToString");

    // Async logging settings
    options.AsyncLogging.EnableAsyncLogging = true;
    options.AsyncLogging.BatchSize = 100;
    options.AsyncLogging.FlushIntervalMs = 5000;
    options.AsyncLogging.MaxQueueSize = 10000;

    // Exception handling
    options.ExceptionHandling.CaptureFullStackTrace = true;
    options.ExceptionHandling.CaptureInnerExceptions = true;
    options.ExceptionHandling.ContinueOnLoggingFailure = true;
});
```

## Advanced Usage

### Custom Context Provider

```csharp
public class CustomActivityLogContextProvider : IActivityLogContextProvider
{
    public Guid? GetCorrelationId()
    {
        // Custom correlation ID logic
        return Activity.Current?.Id != null
            ? Guid.Parse(Activity.Current.Id)
            : Guid.NewGuid();
    }

    public UserContext? GetUserContext()
    {
        // Custom user context logic
        return new UserContext
        {
            UserId = GetCurrentUserId(),
            UserName = GetCurrentUserName()
        };
    }

    public HttpContextInfo? GetHttpContext()
    {
        // Custom HTTP context logic
        return new HttpContextInfo
        {
            Method = HttpContext.Current?.Request.Method,
            Url = HttpContext.Current?.Request.Url?.ToString()
        };
    }
}

// Register custom provider
services.AddScoped<IActivityLogContextProvider, CustomActivityLogContextProvider>();
```

### Custom Serializer

```csharp
public class CustomActivityLogSerializer : IActivityLogSerializer
{
    public string SerializeArguments(MethodInfo method, object?[] arguments)
    {
        // Custom serialization logic
        return JsonSerializer.Serialize(arguments, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    // Implement other methods...
}

// Register custom serializer
services.AddScoped<IActivityLogSerializer, CustomActivityLogSerializer>();
```

## Database Migrations

### SQL Server Migrations

```bash
# Add migration
dotnet ef migrations add InitialCreate --project ActivityLog.DataStorage.SqlServer

# Update database
dotnet ef database update --project ActivityLog.DataStorage.SqlServer
```

### PostgreSQL Migrations

```bash
# Add migration
dotnet ef migrations add InitialCreate --project ActivityLog.DataStorage.PostgreSql

# Update database
dotnet ef database update --project ActivityLog.DataStorage.PostgreSql
```

## Testing

### Repository Pattern Testing

The module uses repository pattern testing for better performance and isolation:

```csharp
public class ActivityLogServiceTests
{
    private readonly Mock<IActivityLogRepository> _mockRepository;
    private readonly ActivityLogService _service;

    public ActivityLogServiceTests()
    {
        _mockRepository = new Mock<IActivityLogRepository>();
        // Setup service with mocked repository
    }

    [Fact]
    public async Task LogMethodExecutionAsync_ShouldPersistActivityLog_WhenValidContext()
    {
        // Arrange
        ActivityLogRecord? capturedLog = null;
        _mockRepository.Setup(x => x.InsertAsync(It.IsAny<ActivityLogRecord>()))
                      .Callback<ActivityLogRecord>(log => capturedLog = log)
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _service.LogMethodExecutionAsync(context);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _mockRepository.Verify(x => x.InsertAsync(It.IsAny<ActivityLogRecord>()), Times.Once);
        capturedLog.ShouldNotBeNull();
        capturedLog.ActivityName.ShouldBe("TestMethod");
    }
}
```

### Integration Testing

```csharp
// Setup for integration tests
services.AddActivityLogging(configuration.GetSection("ActivityLogging"))
        .WithStorage()
        .UseInMemoryDatabase("TestDb");
```

## Performance Considerations

### Minimizing Overhead

1. **Disable logging in production** for non-critical methods
2. **Use filtering** to exclude system namespaces and methods
3. **Enable async logging** to avoid blocking the main thread
4. **Configure appropriate batch sizes** for your workload
5. **Limit serialization depth** for complex objects

### Performance Optimization

```csharp
services.ConfigureActivityLogging(options =>
{
    // Only log slow methods
    options.Performance.LogOnlySlowMethods = true;
    options.Performance.SlowMethodThresholdMs = 500;

    // Optimize serialization
    options.Serialization.MaxDepth = 2;
    options.Serialization.MaxStringLength = 500;

    // Increase batch size for high-throughput scenarios
    options.AsyncLogging.BatchSize = 500;
    options.AsyncLogging.FlushIntervalMs = 2000;
});
```

## Best Practices

1. **Use meaningful activity names** that describe the business operation
2. **Include context in descriptions** using format strings with parameters
3. **Filter sensitive data** by configuring excluded properties and types
4. **Monitor performance impact** and adjust configuration as needed
5. **Use correlation IDs** to track related operations across services
6. **Configure appropriate log levels** for different environments
7. **Regularly clean up old logs** to maintain database performance
8. **Use repository pattern** for testing instead of direct database access
9. **Leverage async logging** for high-throughput scenarios
10. **Configure proper indexes** on frequently queried fields

## Troubleshooting

### Common Issues

1. **High memory usage**: Reduce batch size and serialization depth
2. **Slow performance**: Enable filtering and async logging
3. **Missing logs**: Check configuration and ensure services are properly registered
4. **Serialization errors**: Add problematic types to excluded types list
5. **Repository errors**: Verify database connection and migrations

### Debugging

Enable detailed logging to troubleshoot issues:

```json
{
  "Logging": {
    "LogLevel": {
      "ActivityLog": "Debug",
      "ActivityLog.Services": "Debug",
      "ActivityLog.Interceptors": "Debug"
    }
  }
}
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please read our contributing guidelines and submit pull requests to our GitHub repository.

## Support

For support and questions, please visit our GitHub repository or contact the DotNetBrightener team.