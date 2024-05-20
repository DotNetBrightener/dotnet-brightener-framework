// ReSharper disable CheckNamespace

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public class LoggingOpenTelemetryBuilder
{
    public ILoggingBuilder LoggingBuilder { get; internal set; }

    public IOpenTelemetryBuilder OpenTelemetryBuilder { get; init; }

    public IServiceCollection ServiceCollection { get; init; }

    public Action<OpenTelemetryLoggerOptions> ConfigureOpenTelemetry { get; set; }

    public Action<OtlpExporterOptions> ConfigureOtlpExporterOptions { get; set; }
}

public static class OpenTelemetryServiceCollectionExtensions
{
    public static LoggingOpenTelemetryBuilder EnableOpenTelemetry(this IHostApplicationBuilder hostBuilder,
                                                                  string telemetryEndpoint = null,
                                                                  string telemetryAuthHeader = null)
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
            ConfigureOtlpExporterOptions = (OtlpExporterOptions exporterOptions) =>
            {
                if (!string.IsNullOrEmpty(telemetryEndpoint)) exporterOptions.Endpoint = new Uri(telemetryEndpoint);

                if (!string.IsNullOrEmpty(telemetryAuthHeader))
                    exporterOptions.Headers = $"x-otlp-api-key={telemetryAuthHeader}";
            }
        };

        hostBuilder.Logging.AddOpenTelemetry((options) =>
        {
            options.IncludeScopes           = true;
            options.IncludeFormattedMessage = true;
            options.AddOtlpExporter(otlpExportOptions =>
                                        builder.ConfigureOtlpExporterOptions?.Invoke(otlpExportOptions));

            builder!.ConfigureOpenTelemetry?.Invoke(options);
        });

        builder.ServiceCollection.ConfigureOpenTelemetryMeterProvider(metrics =>
        {
            metrics.AddOtlpExporter(otlpExportOptions =>
                                        builder.ConfigureOtlpExporterOptions?.Invoke(otlpExportOptions));
        });

        builder.ServiceCollection.ConfigureOpenTelemetryTracerProvider(metrics =>
        {
            metrics.AddOtlpExporter(otlpExportOptions =>
                                        builder.ConfigureOtlpExporterOptions?.Invoke(otlpExportOptions));
        });

        hostBuilder.Services.AddSingleton(builder);

        return builder;
    }

    public static LoggingOpenTelemetryBuilder EnableOpenTelemetry(this IServiceCollection serviceCollection,
                                                                  string                  telemetryEndpoint   = null,
                                                                  string                  telemetryAuthHeader = null)
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
            ServiceCollection    = serviceCollection,
            ConfigureOtlpExporterOptions = (OtlpExporterOptions exporterOptions) =>
            {
                if (!string.IsNullOrEmpty(telemetryEndpoint)) exporterOptions.Endpoint = new Uri(telemetryEndpoint);

                if (!string.IsNullOrEmpty(telemetryAuthHeader))
                    exporterOptions.Headers = $"x-otlp-api-key={telemetryAuthHeader}";
            }
        };

        serviceCollection.AddLogging((loggingBuilder) =>
        {
            builder.LoggingBuilder = loggingBuilder.AddOpenTelemetry((options) =>
            {
                options.IncludeScopes           = true;
                options.IncludeFormattedMessage = true;
                options.AddOtlpExporter(otlpExportOptions =>
                                            builder.ConfigureOtlpExporterOptions?.Invoke(otlpExportOptions));


                builder.ConfigureOpenTelemetry?.Invoke(options);
            });
        });


        builder.ServiceCollection.ConfigureOpenTelemetryMeterProvider(metrics =>
        {
            metrics.AddOtlpExporter(otlpExportOptions =>
                                        builder.ConfigureOtlpExporterOptions?.Invoke(otlpExportOptions));
        });

        builder.ServiceCollection.ConfigureOpenTelemetryTracerProvider(metrics =>
        {
            metrics.AddOtlpExporter(otlpExportOptions =>
                                        builder.ConfigureOtlpExporterOptions?.Invoke(otlpExportOptions));
        });

        serviceCollection.AddSingleton(builder);

        return builder;
    }
}