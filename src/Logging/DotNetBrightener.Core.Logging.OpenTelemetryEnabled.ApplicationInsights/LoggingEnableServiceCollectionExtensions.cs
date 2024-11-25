// ReSharper disable CheckNamespace

#nullable enable
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;

namespace Microsoft.Extensions.DependencyInjection;

public static class LoggingEnableServiceCollectionExtensions
{
    public static LoggingOpenTelemetryBuilder AddAzureMonitor(this LoggingOpenTelemetryBuilder builder,
                                                              string                           connectionString,
                                                              TokenCredential?                 credential = null)
    {
        builder.ConfigureOpenTelemetry = options =>
        {
            options.AddAzureMonitorLogExporter(o => o.ConnectionString = connectionString, credential);
        };

        ((OpenTelemetryBuilder)builder.OpenTelemetryBuilder)
           .UseAzureMonitor(o =>
            {
                o.ConnectionString = connectionString;
                o.Credential       = credential ?? new DefaultAzureCredential();
            });

        return builder;
    }
}