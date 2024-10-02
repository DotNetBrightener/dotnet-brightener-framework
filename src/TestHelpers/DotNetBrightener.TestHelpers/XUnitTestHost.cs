using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace DotNetBrightener.TestHelpers;

public class XUnitTestHost
{
    public static IHost CreateTestHost(ITestOutputHelper testOutputHelper, Action<HostBuilderContext, IServiceCollection> configureServices = null)
    {
        // Arrange
        var builder = new HostBuilder()
           .ConfigureServices((hostContext, services) =>
           {
               services.AddLogging(logBuilder =>
               {
                   logBuilder.AddProvider(new XunitLoggerProvider(testOutputHelper));
                   logBuilder.SetMinimumLevel(LogLevel.Debug);
                   logBuilder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
                   logBuilder.AddFilter("Microsoft", LogLevel.Warning);
               });

               configureServices?.Invoke(hostContext, services);
           });

        builder.UseServiceProviderFactory(new ExtendedServiceFactory());

        IHost host = builder.Build();

        return host;
    }
}