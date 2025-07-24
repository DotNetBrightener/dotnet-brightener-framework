using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public class ExtendedServiceFactory : IServiceProviderFactory<IServiceCollection>
{
    private static readonly Type ServiceFactoryType = typeof(IServiceProviderFactory<IServiceCollection>);

    public static void ApplyServiceProviderFactory(IHostBuilder host)
    {
        host.UseServiceProviderFactory(new ExtendedServiceFactory());
    }

    private ExtendedServiceFactory()
    {
#if DEBUG
        if (!Debugger.IsAttached)
            Debugger.Launch();
#endif

    }

    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        return services;
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        ConfigureContainerWithSubFactories(containerBuilder);

        return containerBuilder.BuildServiceProvider();
    }

    private void ConfigureContainerWithSubFactories(IServiceCollection containerBuilder)
    {
        var subContainer = new ServiceCollection();

        var factoryDescriptors = containerBuilder
                                .Where(x => x.ServiceType == ServiceFactoryType ||
                                            x.ImplementationType is not null &&
                                            x.ImplementationType.IsAssignableTo(ServiceFactoryType))
                                .ToList();

        if (factoryDescriptors.Count == 0)
        {
            return;
        }

        factoryDescriptors.ForEach(x =>
        {
            subContainer.Add(ServiceDescriptor.Singleton(ServiceFactoryType, x.ImplementationType!));
        });

        using (ServiceProvider subProvider = subContainer.BuildServiceProvider())
        {
            var factories = subProvider.GetServices<IServiceProviderFactory<IServiceCollection>>();

            foreach (var serviceProviderFactory in factories)
            {
                serviceProviderFactory.CreateBuilder(containerBuilder);
            }
        }

        factoryDescriptors.ForEach(x =>
        {
            containerBuilder.Remove(x);
        });
    }
}