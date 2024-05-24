using System.Reflection;
using DotNetBrightener.Plugins.EventPubSub.AzureServiceBus;
using EventPubSub.WebApiDemo.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services
        // Initialize EventPubSubService
       .AddEventPubSubService()
        // Add Azure Service Bus
       .AddAzureServiceBus(builder.Configuration);

builder.Services.AddEventMessagesFromAssemblies(typeof(DistributedTestMessage).Assembly);
builder.Services.AddEventHandlersFromAssemblies(Assembly.GetExecutingAssembly());

// Add services to the container.

var app = builder.Build();


// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}