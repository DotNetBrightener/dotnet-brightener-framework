using DotNetBrightener.Plugins.EventPubSub;
using EventPubSub.WebApiDemo.Contracts;
using System.Reflection;
using DotNetBrightener.Plugins.EventPubSub.AzureServiceBus.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
       .AddEventPubSubService(Assembly.GetExecutingAssembly(), typeof(DistributedTestMessage).Assembly)
       .AddAzureServiceBus("Endpoint=sb://hs-temp-sb-core.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59hTxGwZnmvIRA4qsl3i1PYsTy/ST+PpRmNDfU+dddA=",
                           "PublisherDemo")
       .AddEventHandlersFromAssemblies([
            Assembly.GetExecutingAssembly(),
        ]);

//.InitMassTransitConfig()
//.WithAzureServiceBus("Endpoint=sb://hs-temp-sb-core.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=59hTxGwZnmvIRA4qsl3i1PYsTy/ST+PpRmNDfU+dddA=")
// // .WithRabbitMq("100.102.153.17", "/", "rabbit", "dj1ig6BVoeATqhJfAuziSk76fLP4QDAU")
//.AddEventHandlersFromAssemblies([
//     Assembly.GetExecutingAssembly(),
// ])
//.Build();

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (IEventPublisher eventPublisher) =>
{
    var eventMessage = new TestMessage
    {
        Name = "world"
    };

    await eventPublisher.Publish(eventMessage);

    var eventMessage2 = new DistributedTestMessage
    {
        Name = " distributed message"
    };

    await eventPublisher.Publish(eventMessage2);

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return eventMessage;
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
