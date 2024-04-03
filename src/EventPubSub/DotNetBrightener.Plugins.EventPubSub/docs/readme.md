# Event Publish/Subscribe Library 

&copy; DotNet Brightener

### Usage

#### Event message

Event message is a message that would be emitted by the `IEventPublisher`. The `IEventHandler` for the message will be proceeded sequentially, and will be stopped if the higher-prioritized handler tells the next one to stop by returning `false` in its `HandleEvent` method.

```csharp
public class YourEventMessage: IEventMessage 
{
	// your model goes here
}
```

#### Non-Stopped event message

Non-stopped event message is the type of `IEventMessage` that will be handled by all the handlers regardless the result from the process of each handler.

```csharp
public class YourEventMessage: INonStoppedEventMessage 
{
	// your model goes here
}
```

#### Define event handler

Define an event handler to process the event emitted by the `IEventPublisher` service as follow:

```csharp 
public class YourEventModelEventHandler: IEventHandler<YourEventMessage>
{
	// the higher number will tell the publisher to execute before the others
	public int Priority { get; } => 10;

	public async Task<bool> HandleEvent(YourEventMessage eventMessage) 
	{
		// do something with your eventMessage

		// if YourEventModel implements INonStoppedEventMessage, 
		// regardless the next statement, 
		// the next handler will continue to process in parallel

		// if you want to let the next handler to process the message
		return true;

		// otherwise, return false here;
		// return false;
	}
}
```

### Emit the event

Inject `IEventPublisher` to your controller / service class and use it as follow:

```csharp 

public class SomeService 
{
	private readonly IEventPublisher _eventPublisher;
	// other services

	public SomeService(IEventPublisher eventPublisher, 
						// other services
						)
	{
		_eventPublisher = eventPublisher;
	}

	public async Task SomeMethod() 
	{
		var eventMessage = new YourEventMessage
		{
			// the event content
		};

		// if you want to let the event message to be processed in the current thread
		await _eventPublisher.Publish(eventMessage);

		// if you want to let the event message to be processed in the another thread
		await _eventPublisher.Publish(eventMessage, true);
	}
}

```

#### Register at startup

You will need to install the package [DotNetBrightener.Plugins.EventPubSub.DependencyInjection](https://www.nuget.org/packages/DotNetBrightener.Plugins.EventPubSub.DependencyInjection) from [nuget.org](https://www.nuget.org) and follow instruction there.