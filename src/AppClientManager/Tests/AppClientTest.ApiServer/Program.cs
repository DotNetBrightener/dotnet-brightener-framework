using DotNetBrightener.Infrastructure.AppClientManager.Middlewares;
using DotNetBrightener.Infrastructure.AppClientManager.Models;
using DotNetBrightener.Infrastructure.AppClientManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var appClientManagerBuilder = builder.Services.AddAppClientManager(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseMiddleware<AppClientCorsEnableMiddleware>();

//app.UseHttpsRedirection();

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

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<IAppClientManager>()
               .CreateAppClient(new AppClient
                {
                    ClientId = "123",
                    AllowedOrigins = "http://localhost:8080;"
                });
}

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
