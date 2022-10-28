using Microsoft.AspNetCore.Hosting;
using NLog;
using NLog.Config;
using NLog.Web;

namespace DotNetBrightener.Core.Logging.Extensions;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder UseLogging(this IWebHostBuilder builder)
    {
        return builder.UseNLog()
                      .ConfigureAppConfiguration((context, configuration) =>
                       {
                           var environment = context.HostingEnvironment;

                           var logConfig = new LoggingConfiguration();

                           var loggingTarget = EventLoggingWatcher.Instance;

                           logConfig.AddRule(LogLevel.Info,
                                             LogLevel.Fatal,
                                             loggingTarget,
                                             "Microsoft.*",
                                             true);

                           logConfig.AddRuleForAllLevels(loggingTarget);
                           NLogBuilder.ConfigureNLog(logConfig);
                           LogManager.Configuration.Variables ["configDir"] = environment.ContentRootPath;
                       });
    }
}