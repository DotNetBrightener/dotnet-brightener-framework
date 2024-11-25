using DotNetBrightener.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace DotNetBrightener.TemplateEngine.Tests.Data;

public static class HostTestingHelper
{
    public static IHost CreateTestHost(ITestOutputHelper          testOutputHelper,
                                       Action<IServiceCollection> configureServices = null)
    {
        var host = XUnitTestHost.CreateTestHost(testOutputHelper,
                                                (context, services) =>
                                                {
                                                    services.AddHttpContextAccessor();

                                                    services.AddTemplateEngine();

                                                    configureServices?.Invoke(services);
                                                });

        return host;
    }
}