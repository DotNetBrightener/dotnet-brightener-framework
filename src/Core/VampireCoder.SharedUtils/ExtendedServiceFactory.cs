using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public class ExtendedServiceFactory : IServiceProviderFactory<IServiceCollection>
{
    private readonly Type _serviceFactoryType = typeof(IServiceProviderFactory<IServiceCollection>);

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
                                .Where(x => x.ServiceType == _serviceFactoryType ||
                                            x.ImplementationType is not null &&
                                            x.ImplementationType.IsAssignableTo(_serviceFactoryType))
                                .ToList();

        if (factoryDescriptors.Count == 0)
        {
            return;
        }

        factoryDescriptors.ForEach(x =>
        {
            subContainer.Add(ServiceDescriptor.Singleton(_serviceFactoryType, x.ImplementationType!));
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