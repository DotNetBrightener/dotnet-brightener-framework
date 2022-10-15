# Extensions for ASP.NET Core of Event Publish/Subscribe Library

&copy; DotNet Brightener

### Usage


#### Register at startup

``` cs
// register the core Event Pub/Sub service
services.AddEventPubSubService();

// other service registrations

```

Then register the implementation of `IEventHandler` by calling

``` cs
services.AddEventHandler<YourEventModelEventHandler>();
```

If you want to automatically let the application detect and register all the implementations of `IEventHandler`, put the following at the end of your `ConfigureServices` method in Startup.cs if you use `Startup.cs` file, or before the application run if you use minimal API.


``` cs 

// before starting the applications

// load all assemblies that are loaded into the application domain
var applicationAssemblies = AppDomain.CurrentDomain.GetAssemblies();

// Register the implementations of IEventHandler from the above assemblies
services.AddEventHandlersFromAssemblies(applicationAssemblies);

```