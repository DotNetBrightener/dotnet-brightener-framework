// ReSharper disable CheckNamespace

using DotNetBrightener.Core.Logging;
using DotNetBrightener.Core.Logging.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Config;
using NLog.Web;

namespace Microsoft.Extensions.DependencyInjection;

public static class LoggingEnableServiceCollectionExtensions
{
    /// <param name="serviceCollection"></param>
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        ///     Configure logging services to the service collection
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public IServiceCollection ConfigureLogging(IConfiguration configuration)
        {
            serviceCollection.AddLogging();

            serviceCollection.AddScoped<IQueueEventLogBackgroundProcessing, NullEventLogBackgroundProcessing>();

            serviceCollection.Configure<LoggingRetentions>(configuration.GetSection(nameof(LoggingRetentions)));

            serviceCollection.AddSingleton<IEventLogWatcher>((provider) =>
            {
                var eventLogWatcher = EventLoggingWatcher.Instance;
                eventLogWatcher.SetServiceScopeFactory(provider.GetService<IServiceScopeFactory>()!);

                return eventLogWatcher;
            });

            serviceCollection.AddHostedService<EventLogQueueBackgroundProcessService>();

            return serviceCollection;
        }
    }

    public static IHostBuilder UseNLogLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseNLog()
                   .ConfigureLogging((context, builder) =>
                    {
                        var hostEnvironment = context.HostingEnvironment;

                        var configuration = context.Configuration;

                        ConfigureNLog(hostEnvironment, configuration);
                    });

        return hostBuilder;
    }

    public static IWebHostBuilder UseNLogLogging(this IWebHostBuilder hostBuilder)
    {
        hostBuilder.UseNLog()
                   .ConfigureLogging((context, builder) =>
                    {
                        var hostEnvironment = context.HostingEnvironment;

                        var configuration = context.Configuration;

                        ConfigureNLog(hostEnvironment, configuration);
                    });

        return hostBuilder;
    }

    private static void ConfigureNLog(IHostEnvironment hostEnvironment, IConfiguration configuration)
    {
        EventLoggingWatcher.Initialize(hostEnvironment, configuration);

        var logConfig = new LoggingConfiguration();

        var loggingTarget = EventLoggingWatcher.Instance;
        var logSettings = configuration
                         .GetSection("Logging:LogLevel")
                         .Get<Dictionary<string, Microsoft.Extensions.Logging.LogLevel>>();

        LogLevel defaultLevel = null;

        foreach (var setting in logSettings)
        {
            if (setting.Key == "Default")
            {
                defaultLevel = setting.Value.ToNLogLevel();

                continue;
            }

            logConfig.AddRule(setting.Value.ToNLogLevel(),
                              LogLevel.Fatal,
                              loggingTarget,
                              setting.Key.Trim('.') + ".*",
                              true);
        }

        if (defaultLevel != null)
        {
            logConfig.AddRule(defaultLevel, LogLevel.Fatal, loggingTarget);
        }


        var lokiEndpoint        = configuration.GetValue<string>("LokiEndpoint");
        var lokiApplicationName = configuration.GetValue<string>("LokiApplicationName");
        var lokiUserName        = configuration.GetValue<string>("LokiUserName");
        var lokiPassword        = configuration.GetValue<string>("LokiPassword");

        if (!string.IsNullOrEmpty(lokiEndpoint))
        {
            loggingTarget.SetLokiTarget(lokiEndpoint, lokiApplicationName, lokiUserName, lokiPassword);
        }

        LogManager.Setup()
                  .LoadConfiguration(logConfig);

        LogManager.Configuration.Variables["configDir"] = hostEnvironment.ContentRootPath;
    }
}