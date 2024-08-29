using DotNetBrightener.Plugins.EventPubSub;
using DotNetBrightener.Plugins.EventPubSub.Distributed.Extensions;
using EventPubSub.WebApiDemo.Contracts;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

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
//eventPubSubConfig.UseAzureServiceBus(builder.Configuration)
//                 .Finalize();

// Add RabbitMq
eventPubSubConfig.UseRabbitMq(builder.Configuration)
                 .Finalize();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();


app.MapGet("/",
           async (IEventPublisher eventPublisher,
                  [FromQuery] bool runInBackground = false) =>
           {
               var eventMessage = new TestMessage
               {
                   Name = "world" + (runInBackground ? " (background)" : "")
               };

               await eventPublisher.Publish(eventMessage, runInBackground);

               var eventMessage2 = new DistributedTestMessage
               {
                   Name = eventMessage.Name
               };

               var response =
                   await eventPublisher
                      .GetResponse<DistributedTestMessageResponse, DistributedTestMessage>(eventMessage2);

               return response;
           });


app.MapGet("/test",
           async (IEventPublisher eventPublisher) =>
           {
               var eventMessage = new SomeUpdateMessage
               {
                   Name = "world"
               };

               await eventPublisher.Publish(eventMessage);

               return "Message sent";
           });

app.Run();