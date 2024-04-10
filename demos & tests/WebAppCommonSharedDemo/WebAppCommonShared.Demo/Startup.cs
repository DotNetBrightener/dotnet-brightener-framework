using AspNet.Extensions.SelfDocumentedProblemResult.ExceptionHandlers;
using DotNetBrightener.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace WebAppCommonShared.Demo;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        services
           .ConfigureLogging(_configuration)
           .AddLogStorage(optionsBuilder =>
            {
                optionsBuilder.UseSqlServer(connectionString);
            });

        services.AddControllers();

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddExceptionHandler<UnhandledExceptionResponseHandler>();

        services.AddProblemDetails();


        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseForwardedHeaders();

        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
        {
            app.UseHttpsRedirection();
        }

        app.UseExceptionHandler();

        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            endpoints.MapClientTelemetryEndpoint("api/telemetry");

            endpoints.MapErrorDocsTrackerUI(options =>
            {
                options.UiPath          = "/errors";
                options.ApplicationName = "Test Error";
            });
        });
    }
}