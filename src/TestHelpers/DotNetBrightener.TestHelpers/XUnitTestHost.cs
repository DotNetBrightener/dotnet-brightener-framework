using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DotNetBrightener.TestHelpers;

public class XUnitTestHost
{
    public static IHost CreateTestHost(ITestOutputHelper?                              testOutputHelper,
                                       Action<HostBuilderContext, IServiceCollection>? configureServices = null)
    {
        // Arrange
        var builder = new HostBuilder();

        ExtendedServiceFactory.ApplyServiceProviderFactory(builder);

        builder.ConfigureServices((hostContext, services) =>
        {
            services.AddLogging(logBuilder =>
            {
                if (testOutputHelper is not null)
                {
                    logBuilder.AddProvider(new XunitLoggerProvider(testOutputHelper));
                }

                logBuilder.AddConsole();
                logBuilder.SetMinimumLevel(LogLevel.Debug);
                logBuilder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                logBuilder.AddFilter("Microsoft", LogLevel.Warning);
            });

            configureServices?.Invoke(hostContext, services);
        });

        IHost host = builder.Build();

        return host;
    }
}