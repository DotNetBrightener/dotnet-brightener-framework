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
                        EventLoggingWatcher.Initialize(context.HostingEnvironment);

                        var logConfig = new LoggingConfiguration();

                        var loggingTarget = EventLoggingWatcher.Instance;

                        var logSettings = context.Configuration
                                                 .GetSection("Logging:LogLevel")
                                                 .Get<Dictionary<string, LogLevel>>();

                        LogLevel defaultLevel = null;

                        foreach (var setting in logSettings)
                        {
                            if (setting.Key == "Default")
                            {
                                defaultLevel = setting.Value;

                                continue;
                            }

                            logConfig.AddRule(setting.Value,
                                              LogLevel.Fatal,
                                              loggingTarget,
                                              setting.Key.Trim('.') + ".*",
                                              true);
                        }

                        if (defaultLevel != null)
                        {
                            logConfig.AddRule(defaultLevel, LogLevel.Fatal, loggingTarget, "*", true);
                        }


                        var lokiEndpoint        = context.Configuration.GetValue<string>("LokiEndpoint");
                        var lokiApplicationName = context.Configuration.GetValue<string>("LokiApplicationName");
                        var lokiUserName        = context.Configuration.GetValue<string>("LokiUserName");
                        var lokiPassword        = context.Configuration.GetValue<string>("LokiPassword");

                        if (!string.IsNullOrEmpty(lokiEndpoint))
                        {
                            loggingTarget.SetLokiTarget(lokiEndpoint, lokiApplicationName, lokiUserName, lokiPassword);
                        }

                        LogManager.Setup().LoadConfiguration(logConfig) ;

                        LogManager.Configuration.Variables["configDir"] = context.HostingEnvironment.ContentRootPath;
                    })
                   .ConfigureServices((hostBuilderContext, serviceCollection) =>
                   {
                       serviceCollection.AddLogging();
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

    public static IWebHostBuilder UseNLogLogging(this IWebHostBuilder hostBuilder)
    {
        hostBuilder.UseNLog()
                   .ConfigureLogging((context, builder) =>
                    {
                        EventLoggingWatcher.Initialize(context.HostingEnvironment);

                        var logConfig = new LoggingConfiguration();

                        var loggingTarget = EventLoggingWatcher.Instance;

                        var logSettings = context.Configuration.GetSection("Logging:LogLevel")
                                                 .Get<Dictionary<string, LogLevel>>();

                        LogLevel defaultLevel = null;

                        foreach (var setting in logSettings)
                        {
                            if (setting.Key == "Default")
                            {
                                defaultLevel = setting.Value;

                                continue;
                            }

                            logConfig.AddRule(setting.Value,
                                              LogLevel.Fatal,
                                              loggingTarget,
                                              setting.Key.Trim('.') + ".*",
                                              true);
                        }

                        if (defaultLevel != null)
                        {
                            logConfig.AddRule(defaultLevel, LogLevel.Fatal, loggingTarget, "*", true);
                        }

                        var lokiEndpoint        = context.Configuration.GetValue<string>("LokiEndpoint");
                        var lokiApplicationName = context.Configuration.GetValue<string>("LokiApplicationName");
                        var lokiUserName        = context.Configuration.GetValue<string>("LokiUserName");
                        var lokiPassword        = context.Configuration.GetValue<string>("LokiPassword");

                        if (!string.IsNullOrEmpty(lokiEndpoint))
                        {
                            loggingTarget.SetLokiTarget(lokiEndpoint, lokiApplicationName, lokiUserName, lokiPassword);
                        }

                        NLogBuilder.ConfigureNLog(logConfig);
                        LogManager.Configuration.Variables["configDir"] = context.HostingEnvironment.ContentRootPath;
                    })
                   .ConfigureServices((hostBuilderContext, serviceCollection) =>
                    {
                        serviceCollection.AddLogging();
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