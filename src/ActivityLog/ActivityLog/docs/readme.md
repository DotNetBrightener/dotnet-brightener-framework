# Activity Logging for .NET

Automatically track and log method execution in your .NET applications with a simple attribute. Perfect for monitoring business operations, debugging issues, and maintaining audit trails.

## What It Does

Activity Logging captures detailed information about your method calls including:
- **Execution timing** - How long methods take to run
- **Input parameters** - What data was passed to methods
- **Return values** - What methods returned
- **Exceptions** - Any errors that occurred
- **User context** - Who performed the action
- **Performance metrics** - Identify slow operations

## Key Benefits

- ✅ **Zero code changes** - Just add `[LogActivity]` attributes
- ✅ **Automatic tracking** - No manual logging code required
- ✅ **Performance monitoring** - Built-in slow method detection
- ✅ **Exception capture** - Comprehensive error tracking
- ✅ **Async support** - Works with async/await methods
- ✅ **Configurable** - Control what gets logged and how
- ✅ **Multiple databases** - SQL Server, PostgreSQL, or in-memory

## Quick Start

### 1. Install the Package

```bash
# Core module
dotnet add package DotNetBrightener.ActivityLog

# Choose a database provider (optional)
dotnet add package DotNetBrightener.ActivityLog.DataStorage.SqlServer
# OR
dotnet add package DotNetBrightener.ActivityLog.DataStorage.PostgreSql
```

### 2. Configure Your Application

Add this to your `Program.cs` or `Startup.cs`:

```csharp
// Basic setup (logs to console/file only)
services.AddActivityLogging();

// With database storage
services.AddActivityLogging()
        .WithStorage()
        .UseSqlServer("your-connection-string");
```

### 3. Add Configuration (Optional)

Add to your `appsettings.json`:

```json
{
  "ActivityLogging": {
    "IsEnabled": true,
    "MinimumLogLevel": "Information",
    "Performance": {
      "SlowMethodThresholdMs": 1000
    },
    "Serialization": {
      "ExcludedProperties": ["Password", "Secret", "Token"]
    }
  }
}
```

### 4. Register Your Services

```csharp
// Register services that should be logged
services.AddActivityLoggedService<IUserService, UserService>();
services.AddActivityLoggedService<IOrderService, OrderService>();
```

### 5. Start Logging

Add the `[LogActivity]` attribute to methods you want to track:

```csharp
public class UserService : IUserService
{
    [LogActivity("GetUser", "Retrieved user {userId}")]
    public async Task<User> GetUserAsync(int userId)
    {
        return await _repository.GetByIdAsync(userId);
    }
}
```

That's it! Your methods are now being logged automatically.

## Basic Usage Examples

### Simple Method Logging

```csharp
public class UserService : IUserService
{
    [LogActivity("GetUser", "Getting user {userId}")]
    public async Task<User> GetUserAsync(int userId)
    {
        return await _repository.GetByIdAsync(userId);
    }

    [LogActivity("CreateUser", "Creating user {request.Name}")]
    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User { Name = request.Name, Email = request.Email };
        return await _repository.CreateAsync(user);
    }
}
```

### Different Logging Scenarios

```csharp
public class OrderService : IOrderService
{
    // Log with custom activity name
    [LogActivity("ProcessOrder")]
    public async Task ProcessOrderAsync(int orderId)
    {
        // Implementation
    }

    // Log with description template
    [LogActivity("CancelOrder", "Cancelled order {orderId} for user {userId}")]
    public async Task CancelOrderAsync(int orderId, int userId)
    {
        // Implementation
    }

    // Log synchronous methods too
    [LogActivity("ValidateOrder", "Validating order data")]
    public bool ValidateOrder(Order order)
    {
        return order.IsValid();
    }
}
```

## Adding Runtime Context

You can add extra information to your logs during method execution:

```csharp
public class OrderService : IOrderService
{
    [LogActivity("ProcessPayment", "Processing payment for order {orderId}")]
    public async Task ProcessPaymentAsync(int orderId, decimal amount)
    {
        // Add custom metadata during execution
        ActivityLogContext.AddMetadata("paymentAmount", amount);
        ActivityLogContext.AddMetadata("processingStartTime", DateTime.UtcNow);

        // Process payment logic here
        var result = await _paymentService.ProcessAsync(orderId, amount);

        // Add more context based on results
        ActivityLogContext.AddMetadata("paymentResult", result.Status);
        ActivityLogContext.AddMetadata("transactionId", result.TransactionId);

        // You can even modify the activity details
        if (result.IsSuccess)
        {
            ActivityLogContext.SetActivityDescription($"Successfully processed ${amount} payment");
        }
        else
        {
            ActivityLogContext.SetActivityDescription($"Failed to process ${amount} payment: {result.Error}");
        }
    }
}
```

### Batch Metadata Addition

```csharp
[LogActivity("AnalyzeData", "Analyzing customer data")]
public async Task AnalyzeCustomerDataAsync(int customerId)
{
    // Add multiple pieces of metadata at once
    var metadata = new Dictionary<string, object?>
    {
        ["customerId"] = customerId,
        ["analysisType"] = "comprehensive",
        ["startTime"] = DateTime.UtcNow,
        ["version"] = "2.1"
    };

    ActivityLogContext.AddMetadata(metadata);

    // Your analysis logic here
}
```

## Configuration Options

### Basic Configuration

Control what gets logged and how:

```json
{
  "ActivityLogging": {
    "IsEnabled": true,
    "MinimumLogLevel": "Information"
  }
}
```

### Performance Settings

Monitor and optimize slow methods:

```json
{
  "ActivityLogging": {
    "Performance": {
      "SlowMethodThresholdMs": 1000,
      "LogOnlySlowMethods": false,
      "EnableHighPrecisionTiming": true
    }
  }
}
```

### Data Protection

Exclude sensitive information from logs:

```json
{
  "ActivityLogging": {
    "Serialization": {
      "ExcludedProperties": ["Password", "Secret", "Token", "ApiKey"],
      "ExcludedTypes": ["System.IO.Stream", "Microsoft.AspNetCore.Http.HttpContext"],
      "MaxDepth": 3,
      "MaxStringLength": 1000,
      "SerializeInputParameters": true,
      "SerializeReturnValues": true
    }
  }
}
```

### Filtering Options

Control which methods get logged:

```json
{
  "ActivityLogging": {
    "Filtering": {
      "ExcludedNamespaces": ["System", "Microsoft"],
      "ExcludedMethods": ["ToString", "GetHashCode", "Equals"],
      "UseWhitelistMode": false
    }
  }
}
```

### Background Processing

Configure async logging for better performance:

```json
{
  "ActivityLogging": {
    "AsyncLogging": {
      "EnableAsyncLogging": true,
      "BatchSize": 100,
      "FlushIntervalMs": 5000,
      "MaxQueueSize": 10000
    }
  }
}
```

## Common Scenarios

### E-commerce Application

```csharp
public class OrderService : IOrderService
{
    [LogActivity("CreateOrder", "Creating order for customer {customerId}")]
    public async Task<Order> CreateOrderAsync(int customerId, List<OrderItem> items)
    {
        ActivityLogContext.AddMetadata("itemCount", items.Count);
        ActivityLogContext.AddMetadata("totalAmount", items.Sum(i => i.Price * i.Quantity));

        var order = await _repository.CreateAsync(new Order
        {
            CustomerId = customerId,
            Items = items
        });

        ActivityLogContext.AddMetadata("orderId", order.Id);
        return order;
    }

    [LogActivity("ProcessPayment", "Processing payment for order {orderId}")]
    public async Task<PaymentResult> ProcessPaymentAsync(int orderId, PaymentInfo payment)
    {
        ActivityLogContext.AddMetadata("paymentMethod", payment.Method);
        ActivityLogContext.AddMetadata("amount", payment.Amount);

        try
        {
            var result = await _paymentService.ProcessAsync(payment);
            ActivityLogContext.AddMetadata("transactionId", result.TransactionId);
            ActivityLogContext.AddMetadata("status", result.Status);
            return result;
        }
        catch (PaymentException ex)
        {
            ActivityLogContext.AddMetadata("paymentError", ex.Message);
            throw;
        }
    }
}
```

### User Management

```csharp
public class UserService : IUserService
{
    [LogActivity("RegisterUser", "Registering new user {email}")]
    public async Task<User> RegisterUserAsync(string email, string password)
    {
        ActivityLogContext.AddMetadata("registrationSource", "web");
        ActivityLogContext.AddMetadata("timestamp", DateTime.UtcNow);

        var user = await _repository.CreateAsync(new User { Email = email });

        ActivityLogContext.SetTargetEntity($"User:{user.Id}");
        return user;
    }

    [LogActivity("UpdateProfile", "Updating profile for user {userId}")]
    public async Task UpdateProfileAsync(int userId, UserProfile profile)
    {
        ActivityLogContext.AddMetadata("fieldsUpdated", profile.GetChangedFields());
        ActivityLogContext.SetTargetEntity($"User:{userId}");

        await _repository.UpdateAsync(userId, profile);
    }
}
```

## Database Setup

### SQL Server

```csharp
// In Program.cs
services.AddActivityLogging()
        .WithStorage()
        .UseSqlServer("Server=localhost;Database=MyApp;Trusted_Connection=true;");
```

Run migrations to create the database tables:

```bash
dotnet ef database update --project YourProject
```

### PostgreSQL

```csharp
// In Program.cs
services.AddActivityLogging()
        .WithStorage()
        .UsePostgreSql("Host=localhost;Database=myapp;Username=postgres;Password=password");
```

### In-Memory (for Testing)

```csharp
// Perfect for unit tests and development
services.AddActivityLogging()
        .WithStorage()
        .UseInMemoryDatabase("TestDatabase");
```

## Best Practices

### 1. Use Meaningful Activity Names

```csharp
// ✅ Good - describes the business operation
[LogActivity("ProcessRefund", "Processing refund for order {orderId}")]

// ❌ Avoid - too generic
[LogActivity("DoWork", "Doing some work")]
```

### 2. Include Important Context

```csharp
[LogActivity("SendEmail", "Sending {emailType} email to {recipientEmail}")]
public async Task SendEmailAsync(string emailType, string recipientEmail, string content)
{
    // Add extra context during execution
    ActivityLogContext.AddMetadata("emailSize", content.Length);
    ActivityLogContext.AddMetadata("templateVersion", GetTemplateVersion(emailType));

    await _emailService.SendAsync(recipientEmail, content);
}
```

### 3. Protect Sensitive Data

```json
{
  "ActivityLogging": {
    "Serialization": {
      "ExcludedProperties": [
        "Password", "Secret", "Token", "ApiKey",
        "CreditCardNumber", "SSN", "PersonalData"
      ]
    }
  }
}
```

### 4. Monitor Performance Impact

```json
{
  "ActivityLogging": {
    "Performance": {
      "LogOnlySlowMethods": true,
      "SlowMethodThresholdMs": 500
    },
    "AsyncLogging": {
      "EnableAsyncLogging": true
    }
  }
}
```

### 5. Use Appropriate Log Levels

```csharp
// Critical business operations
[LogActivity("ProcessPayment", "Processing payment", LogLevel = ActivityLogLevel.Warning)]

// Regular operations
[LogActivity("GetUser", "Getting user data", LogLevel = ActivityLogLevel.Information)]

// Detailed debugging
[LogActivity("ValidateInput", "Validating input", LogLevel = ActivityLogLevel.Debug)]
```

## Performance Tips

### For High-Traffic Applications

```json
{
  "ActivityLogging": {
    "Performance": {
      "LogOnlySlowMethods": true,
      "SlowMethodThresholdMs": 500
    },
    "AsyncLogging": {
      "EnableAsyncLogging": true,
      "BatchSize": 500,
      "FlushIntervalMs": 2000
    },
    "Serialization": {
      "MaxDepth": 2,
      "MaxStringLength": 500
    }
  }
}
```

### Exclude Noisy Methods

```json
{
  "ActivityLogging": {
    "Filtering": {
      "ExcludedNamespaces": ["System", "Microsoft", "Newtonsoft"],
      "ExcludedMethods": ["ToString", "GetHashCode", "Equals", "Dispose"]
    }
  }
}
```

### Development vs Production

```json
// Development - log everything
{
  "ActivityLogging": {
    "IsEnabled": true,
    "MinimumLogLevel": "Debug",
    "Performance": {
      "LogOnlySlowMethods": false
    }
  }
}

// Production - log only important operations
{
  "ActivityLogging": {
    "IsEnabled": true,
    "MinimumLogLevel": "Information",
    "Performance": {
      "LogOnlySlowMethods": true,
      "SlowMethodThresholdMs": 1000
    }
  }
}
```

## Troubleshooting

### Nothing is Being Logged

**Check these common issues:**

1. **Service registration missing**:
   ```csharp
   // Make sure you have this
   services.AddActivityLogging();
   services.AddActivityLoggedService<IYourService, YourService>();
   ```

2. **Logging is disabled**:
   ```json
   {
     "ActivityLogging": {
       "IsEnabled": true  // Make sure this is true
     }
   }
   ```

3. **Log level too high**:
   ```json
   {
     "ActivityLogging": {
       "MinimumLogLevel": "Debug"  // Try lowering this
     }
   }
   ```

### Performance Issues

**If logging is slowing down your app:**

1. **Enable async logging**:
   ```json
   {
     "ActivityLogging": {
       "AsyncLogging": {
         "EnableAsyncLogging": true
       }
     }
   }
   ```

2. **Log only slow methods**:
   ```json
   {
     "ActivityLogging": {
       "Performance": {
         "LogOnlySlowMethods": true,
         "SlowMethodThresholdMs": 1000
       }
     }
   }
   ```

3. **Reduce serialization**:
   ```json
   {
     "ActivityLogging": {
       "Serialization": {
         "MaxDepth": 1,
         "SerializeInputParameters": false,
         "SerializeReturnValues": false
       }
     }
   }
   ```

### Serialization Errors

**If you get serialization exceptions:**

1. **Exclude problematic types**:
   ```json
   {
     "ActivityLogging": {
       "Serialization": {
         "ExcludedTypes": [
           "System.IO.Stream",
           "Microsoft.AspNetCore.Http.HttpContext",
           "YourApp.ProblematicType"
         ]
       }
     }
   }
   ```

2. **Exclude sensitive properties**:
   ```json
   {
     "ActivityLogging": {
       "Serialization": {
         "ExcludedProperties": ["Password", "Secret", "InternalData"]
       }
     }
   }
   ```

### Database Connection Issues

**If logs aren't being saved to the database:**

1. **Check connection string**:
   ```csharp
   services.AddActivityLogging()
           .WithStorage()
           .UseSqlServer("your-connection-string-here");
   ```

2. **Run database migrations**:
   ```bash
   dotnet ef database update
   ```

3. **Test with in-memory database first**:
   ```csharp
   services.AddActivityLogging()
           .WithStorage()
           .UseInMemoryDatabase("TestDb");
   ```

### Getting Help

**Enable debug logging to see what's happening:**

```json
{
  "Logging": {
    "LogLevel": {
      "ActivityLog": "Debug"
    }
  }
}
```

This will show detailed information about what the activity logging system is doing, helping you identify any issues.

## Support

For questions and support:
- Check the troubleshooting section above
- Review the configuration examples
- Enable debug logging to see detailed information
- Visit our GitHub repository for issues and discussions