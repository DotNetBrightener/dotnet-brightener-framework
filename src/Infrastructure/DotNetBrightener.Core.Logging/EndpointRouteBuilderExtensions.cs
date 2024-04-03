#nullable enable
using DotNetBrightener.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    ///     Maps the telemetry endpoint for client-side telemetry.
    /// </summary>
    /// <param name="builder">
    ///     The endpoint route builder
    /// </param>
    /// <param name="endpoint">
    ///     The group endpoint path to map the telemetry endpoint to.
    /// </param>
    /// <remarks>
    ///     Need to configure JSON serialization settings to handle the LogLevel enum.
    ///
    ///     For example:
    ///
    ///     <code>
    ///     services.ConfigureHttpJsonOptions(options =>
    ///     {
    ///         options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    ///     });
    /// 
    ///     services.Configure&lt;JsonOptions&gt;(options =>
    ///     {
    ///         options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    ///     }); 
    ///     </code>
    /// </remarks>
    /// <returns></returns>
    public static IEndpointRouteBuilder MapClientTelemetryEndpoint(this IEndpointRouteBuilder builder,
                                                                   string endpoint = "api/client-telemetry")
    {
        var group = builder.MapGroup(endpoint);

        group.MapPost("{loggerName}",
                      async (string loggerName,
                             [FromBody]
                             ClientTelemetryModel message,
                             ILoggerFactory       loggerFactory) =>
                      {
                          var logger = loggerFactory.CreateLogger(loggerName);

                          if (!string.IsNullOrEmpty(message.StackTrace))
                          {
                              logger.Log(message.Level,
                                         new StackTraceOnlyException(message.StackTrace),
                                         message.Message,
                                         message.Properties);
                          }
                          else
                          {
                              logger.Log(message.Level, message.Message, message.Properties);
                          }
                      });

        return builder;
    }
}