using System.IO;
using Microsoft.AspNetCore.Hosting;
using NLog;
using NLog.LayoutRenderers;
using NLog.Web;

namespace DotNetBrightener.Core.Logging;

public static class WebHostBuilderExtensions
{
    public static IWebHostBuilder UseNLogWeb(this IWebHostBuilder builder)
    {
        LayoutRenderer.Register<TenantLayoutRenderer>(TenantLayoutRenderer.LayoutRendererName);
        builder.UseNLog();
        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            var environment = context.HostingEnvironment;

            NLogBuilder.ConfigureNLog($"{environment.ContentRootPath}{Path.DirectorySeparatorChar}NLog.config");
                                                  
            LogManager.Configuration.Variables["configDir"] = environment.ContentRootPath;
        });

        return builder;
    }
}