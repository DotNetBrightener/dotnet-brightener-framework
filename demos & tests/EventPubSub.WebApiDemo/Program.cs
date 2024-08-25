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

var massTransitConfigurator = eventPubSubConfig.EnableMassTransit();

// Add Azure Service Bus
// massTransitConfigurator.UseAzureServiceBus(builder.Configuration);

// Add RabbitMq
massTransitConfigurator.UseRabbitMq(builder.Configuration);

massTransitConfigurator.Finalize();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();


app.MapGet("/", async (IEventPublisher eventPublisher) =>
{
    var eventMessage = new TestMessage
    {
        Name = "world"
    };

    await eventPublisher.Publish(eventMessage);

    var eventMessage2 = new DistributedTestMessage
    {
        Name = eventMessage.Name
    };

    var response = await eventPublisher.GetResponse<DistributedTestMessageResponse, DistributedTestMessage>(eventMessage2);
    
    return response;
});

app.Run();