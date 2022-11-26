// ReSharper disable CheckNamespace

using DotNetBrightener.Core.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Web;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection;

public static class LoggingEnableServiceCollectionExtensions
{
    public static IHostBuilder UseNLogLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseNLog()
                   .ConfigureLogging((context, builder) =>
                    {
                        var logConfig = new LoggingConfiguration();

                        var loggingTarget = EventLoggingWatcher.Instance;

                        var logSettings = context.Configuration.GetSection("Logging:LogLevel")
                                                 .Get<Dictionary<string, LogLevel>>();

                        var lokiEndpoint = context.Configuration.GetValue<string>("LokiEndpoint");

                        foreach (var setting in logSettings)
                        {
                            if (setting.Key == "Default")
                            {
                                logConfig.AddRule(setting.Value, LogLevel.Fatal, loggingTarget, "*", true);

                                continue;
                            }

                            logConfig.AddRule(setting.Value,
                                              LogLevel.Fatal,
                                              loggingTarget,
                                              setting.Key.Trim('.') + ".*",
                                              true);
                        }

                        if (!string.IsNullOrEmpty(lokiEndpoint))
                        {
                            loggingTarget.SetLokiTarget(lokiEndpoint);
                        }

                        NLogBuilder.ConfigureNLog(logConfig);
                        LogManager.Configuration.Variables["configDir"] = context.HostingEnvironment.ContentRootPath;
                    })
                   .ConfigureServices((hostBuilderContext, serviceCollection) =>
                    {
                        serviceCollection.AddScoped<IEventLogDataService, EventLogDataService>();
                        serviceCollection
                           .AddScoped<IQueueEventLogBackgroundProcessing, QueueEventLogBackgroundProcessing>();

                        serviceCollection.AddSingleton<IEventLogWatcher>((provider) =>
                        {
                            var eventLogWatcher = EventLoggingWatcher.Instance;
                            eventLogWatcher.SetServiceScopeFactory(provider.GetService<IServiceScopeFactory>()!);

                            return eventLogWatcher;
                        });
                    });

        return hostBuilder;
    }
}