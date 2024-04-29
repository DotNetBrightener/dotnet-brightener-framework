# Extensions for ASP.NET Core of Event Publish/Subscribe Library

&copy; 2024 DotNet Brightener

![NuGet Version](https://img.shields.io/nuget/v/DotNetBrightener.Plugins.EventPubSub.DependencyInjection)

### Usage


#### Register at startup

```csharp
// register the core Event Pub/Sub service
services.AddEventPubSubService();

// other service registrations

```

Then register the implementation of `IEventHandler` by calling

```csharp
services.AddEventHandler<YourEventModelEventHandler>();
```

If you want to automatically let the application detect and register all the implementations of `IEventHandler`, put the following at the end of your `ConfigureServices` method in Startup.cs if you use `Startup.cs` file, or before the application run if you use `Program.cs` file.


```csharp

// before starting the applications

// load all assemblies that are loaded into the application domain
var applicationAssemblies = AppDomain.CurrentDomain.GetAssemblies();

// Register the implementations of IEventHandler from the above assemblies
services.AddEventHandlersFromAssemblies(applicationAssemblies);

```