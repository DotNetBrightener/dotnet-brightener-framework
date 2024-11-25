using EventPubSub.WebApiDemo.Contracts;
using System.Reflection;
using DotNetBrightener.Plugins.EventPubSub;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Initialize EventPubSubService
var eventPubSubConfig = builder.Services
                               .AddEventPubSubService()
                                // scan for event messages in the given assembly
                               .AddEventMessagesFromAssemblies(typeof(DistributedTestMessage).Assembly)
                                // scan for event handlers in the given assembly
                               .AddEventHandlersFromAssemblies(Assembly.GetExecutingAssembly());

// Add Azure Service Bus
eventPubSubConfig.AddAzureServiceBus(builder.Configuration);

var app = builder.Build();


// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/", () => "Consumer launched. Watch the console for incoming messages.");

app.MapGet("/test", async (IEventPublisher eventPublisher) =>
{
    var message = new SomeUpdateMessageFromNative
    {
        Name = "From native consumer app"
    };

    await eventPublisher.Publish(message);

    return "Message sent";
});

app.Run();