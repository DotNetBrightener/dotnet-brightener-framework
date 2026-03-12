using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.DataAccess.EF.PostgreSQL;

internal class UuidV7EnablerForPostgreSqlDbContexts<TDbContext> : IServiceProviderFactory<IServiceCollection> where TDbContext : DbContext
{
    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        var dbContextDescriptors = services.Where(x => x.ServiceType.IsAssignableTo(typeof(DbContext)) ||
                                                       x.ImplementationType is not null &&
                                                       x.ImplementationType.IsAssignableTo(typeof(TDbContext)))
                                           .ToList();

        foreach (var descriptor in dbContextDescriptors)
        {
            services.Replace(new ServiceDescriptor(
                                                   descriptor.ServiceType,
                                                   provider => InitializeUuidV7ForDbContext(provider, descriptor),
                                                   descriptor.Lifetime));
        }

        return services;
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        return containerBuilder.BuildServiceProvider();
    }

    private static object InitializeUuidV7ForDbContext(IServiceProvider provider, ServiceDescriptor descriptor)
    {
        // Resolve the original service instance
        object serviceInstance;

        if (descriptor.ImplementationInstance != null)
        {
            return descriptor.ImplementationInstance;
        }

        if (descriptor.ImplementationFactory != null)
        {
            serviceInstance = descriptor.ImplementationFactory(provider);
        }
        else
        {
            serviceInstance = ActivatorUtilities.CreateInstance(provider,
                                                                descriptor.ImplementationType ?? descriptor.ServiceType);
        }

        if (serviceInstance is TDbContext dbContextInstance)
        {
            PostgresFunctionInitializer.InitializeUuidV7Function(dbContextInstance);
        }

        return serviceInstance;
    }
}