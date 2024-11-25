using DotNetBrightener.Plugins.EventPubSub;
using EventPubSub.WebApiDemo.Contracts;
using System.Reflection;

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
eventPubSubConfig.UseAzureServiceBus(builder.Configuration)
                 .Finalize();

// Add RabbitMq
//eventPubSubConfig.UseRabbitMq(builder.Configuration)
//                 .Finalize();

var app = builder.Build();


// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapGet("/", () => "Consumer launched. Watch the console for incoming messages.");

app.Run();