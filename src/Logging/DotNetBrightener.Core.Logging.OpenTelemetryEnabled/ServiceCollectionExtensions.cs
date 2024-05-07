// ReSharper disable CheckNamespace

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public class LoggingOpenTelemetryBuilder
{
    public ILoggingBuilder LoggingBuilder { get; internal set; }

    public IOpenTelemetryBuilder OpenTelemetryBuilder { get; init; }

    public IServiceCollection                 ServiceCollection      { get; init; }

    public Action<OpenTelemetryLoggerOptions> ConfigureOpenTelemetry { get; set; }
}

public static class OpenTelemetryServiceCollectionExtensions
{
    public static LoggingOpenTelemetryBuilder EnableOpenTelemetry(this IHostApplicationBuilder hostBuilder)
    {
        var telemetryBuilder = hostBuilder.Services
                                          .AddOpenTelemetry()
                                          .WithMetrics(x =>
                                           {
                                               x.AddAspNetCoreInstrumentation()
                                                .AddHttpClientInstrumentation();
                                           })
                                          .WithTracing(x =>
                                           {
                                               x.AddAspNetCoreInstrumentation()
                                                .AddHttpClientInstrumentation();
                                           });

        var builder = new LoggingOpenTelemetryBuilder
        {
            LoggingBuilder       = hostBuilder.Logging,
            ServiceCollection    = hostBuilder.Services,
            OpenTelemetryBuilder = telemetryBuilder,
        };

        hostBuilder.Logging.AddOpenTelemetry((options) =>
        {
            options.IncludeScopes           = true;
            options.IncludeFormattedMessage = true;

            builder!.ConfigureOpenTelemetry?.Invoke(options);
        });

        hostBuilder.Services.AddSingleton(builder);

        return builder;
    }

    public static LoggingOpenTelemetryBuilder EnableOpenTelemetry(this IServiceCollection serviceCollection)
    {

        var telemetryBuilder = serviceCollection
                                          .AddOpenTelemetry()
                                          .WithMetrics(x =>
                                           {
                                               x.AddAspNetCoreInstrumentation()
                                                .AddHttpClientInstrumentation();
                                           })
                                          .WithTracing(x =>
                                           {
                                               x.AddAspNetCoreInstrumentation()
                                                .AddHttpClientInstrumentation();
                                           });
        
        var builder = new LoggingOpenTelemetryBuilder
        {
            OpenTelemetryBuilder = telemetryBuilder,
            ServiceCollection    = serviceCollection
        };

        serviceCollection.AddLogging((loggingBuilder) =>
        {
            builder.LoggingBuilder = loggingBuilder.AddOpenTelemetry((options) =>
            {
                options.IncludeScopes           = true;
                options.IncludeFormattedMessage = true;

                builder.ConfigureOpenTelemetry?.Invoke(options);
            });
        });


        serviceCollection.AddSingleton(builder);

        return builder;
    }
}