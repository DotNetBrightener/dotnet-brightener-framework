using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNetBrightener.TemplateEngine.Tests.Data;

public static class HostTestingHelper
{
    public static IHost CreateTestHost(Action<IServiceCollection> configureServices = null)
    {
        var builder = new HostBuilder()
           .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpContextAccessor();
                services.AddLogging();
                services.AddTemplateEngine();
                configureServices?.Invoke(services);
            });

        var host = builder.Build();

        return host;
    }
}