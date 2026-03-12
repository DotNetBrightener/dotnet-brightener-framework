# Activity Log Database Storage

Adds persistent Layer for [`Activity Log`](https://www.nuget.org/packages/DotNetBrightener.ActivityLog) package.

## Quick Setup

### 1. Choose Your Database

**SQL Server** (Recommended for Windows/Azure):
```bash
dotnet add package DotNetBrightener.ActivityLog.DataStorage.SqlServer
```

**PostgreSQL** (Recommended for Linux/Cross-platform):
```bash
dotnet add package DotNetBrightener.ActivityLog.DataStorage.PostgreSql
```

**In-Memory** (For testing only):
```bash
# No additional package needed - built into this package
```

### 2. Configure Your Connection

**SQL Server**:
```csharp
// In Program.cs or Startup.cs
services.AddActivityLogging()
        .WithStorage()
        .UseSqlServer("Server=localhost;Database=MyApp;Trusted_Connection=true;");
```

**PostgreSQL**:
```csharp
// In Program.cs or Startup.cs
services.AddActivityLogging()
        .WithStorage()
        .UsePostgreSql("Host=localhost;Database=MyApp;Username=myuser;Password=mypass");
```

**In-Memory** (for testing):
```csharp
// Perfect for unit tests and development
services.AddActivityLogging()
        .WithStorage()
        .UseInMemoryDatabase("TestDatabase");
```

### 3. Create the Database Tables

The system will automatically create the necessary tables when your application starts.

## What Gets Stored

Every time a method with `[LogActivity]` runs, a record is saved with:

- **When it happened** - Start time, end time, duration
- **What method ran** - Class name, method name, namespace
- **Who did it** - User ID, username (if available)
- **What data was used** - Input parameters and return values
- **How it went** - Success/failure, any exceptions
- **Extra context** - Custom metadata you add during execution
- **Performance info** - Execution time, slow method detection

