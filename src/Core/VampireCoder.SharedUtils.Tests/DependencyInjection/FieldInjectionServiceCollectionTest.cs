using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VampireCoder.SharedUtils.DependencyInjection;

namespace VampireCoder.SharedUtils.Tests.DependencyInjection;

public interface ITestService1;

public class TestService1 : ITestService1;

public interface ITestService2
{
    ITestService1 ServiceInjectedViaConstructor { get; }

    ITestService1 ServiceInjectedViaField { get; }
}

public class TestService2 : ITestService2
{
    private ITestService1 _testService1InjectableViaConstructor;

    [Injectable]
    private ITestService1 _testService1Injectable;

    public TestService2(ITestService1 service1)
    {
        _testService1InjectableViaConstructor = service1;
    }

    public ITestService1 ServiceInjectedViaConstructor => _testService1InjectableViaConstructor;

    public ITestService1 ServiceInjectedViaField => _testService1Injectable;
}

public class FieldInjectionServiceCollectionTests
{
    private readonly IServiceProvider serviceProvider;

    public FieldInjectionServiceCollectionTests()
    {
        IServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddScoped<ITestService1, TestService1>();
        serviceCollection.AddScoped<ITestService2, TestService2>();

        serviceCollection.EnableFieldInjectionResolution();
        serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public async Task FieldInjection_ShouldNotChangeBehaviour_Of_ServiceCollection()
    {
        var testService2 = serviceProvider.GetRequiredService<ITestService2>();

        testService2.ServiceInjectedViaConstructor.Should().NotBeNull();
        testService2.ServiceInjectedViaField.Should().NotBeNull();
        testService2.ServiceInjectedViaField.Should().BeSameAs(testService2.ServiceInjectedViaConstructor);

        var testService1 = serviceProvider.GetRequiredService<ITestService1>();

        testService1.Should()
                    .BeSameAs(testService2.ServiceInjectedViaField);
    }

    [Fact]
    public async Task FieldInjectionWhenResolving_ShouldCacheTheFields()
    {
        using (var scope = serviceProvider.CreateScope())
        {
            FieldInjectionEnableServiceCollectionExtensions.InjectableFields
                                                           .Should()
                                                           .BeEmpty();

            var testService2 = scope.ServiceProvider.GetRequiredService<ITestService2>();

            testService2.ServiceInjectedViaConstructor.Should().NotBeNull();
            testService2.ServiceInjectedViaField.Should().NotBeNull();
            testService2.ServiceInjectedViaField.Should().BeSameAs(testService2.ServiceInjectedViaConstructor);

            FieldInjectionEnableServiceCollectionExtensions.InjectableFields
                                                           .Count
                                                           .Should()
                                                           .Be(2);
        }

        using (var scope = serviceProvider.CreateScope())
        {
            FieldInjectionEnableServiceCollectionExtensions.InjectableFields
                                                           .Count
                                                           .Should()
                                                           .Be(2);

            var testService2 = scope.ServiceProvider.GetRequiredService<ITestService2>();

            testService2.ServiceInjectedViaConstructor.Should().NotBeNull();
            testService2.ServiceInjectedViaField.Should().NotBeNull();
            testService2.ServiceInjectedViaField.Should().BeSameAs(testService2.ServiceInjectedViaConstructor);

            FieldInjectionEnableServiceCollectionExtensions.InjectableFields
                                                           .Count
                                                           .Should()
                                                           .Be(2);
        }
    }
}