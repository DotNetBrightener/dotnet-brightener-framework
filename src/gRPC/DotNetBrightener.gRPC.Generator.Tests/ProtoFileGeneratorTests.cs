using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.gRPC.Generator.Tests;


public class ProtoFileGeneratorTests
{
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();

        _serviceProvider = serviceCollection.BuildServiceProvider();

    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }
}