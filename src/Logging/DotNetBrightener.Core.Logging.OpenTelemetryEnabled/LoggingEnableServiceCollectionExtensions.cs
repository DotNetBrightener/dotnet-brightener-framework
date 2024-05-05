// ReSharper disable CheckNamespace

using Azure.Core;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public static class LoggingEnableServiceCollectionExtensions
{
    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder,
                                                                 string                       connectionString,
                                                                 TokenCredential?             credential = null)
    {
        builder.Logging.AddOpenTelemetry((options) =>
        {
            options.IncludeScopes           = true;
            options.IncludeFormattedMessage = true;
            options.AddAzureMonitorLogExporter(o => o.ConnectionString = connectionString, credential);

        });

        builder.Services.AddOpenTelemetry()
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

        return builder;
    }
}