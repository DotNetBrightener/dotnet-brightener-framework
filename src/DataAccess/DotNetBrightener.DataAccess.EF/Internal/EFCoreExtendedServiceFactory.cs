using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetBrightener.DataAccess.EF.Internal;

public class EFCoreExtendedServiceFactory : IServiceProviderFactory<IServiceCollection>
{
    internal static readonly ConcurrentDictionary<Type, List<FieldInfo>> InjectableFields = new();

    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        var dbContextDescriptors = services.Where(x => x.ServiceType.IsAssignableTo(typeof(DbContext)) ||
                                                       x.ImplementationType is not null &&
                                                       x.ImplementationType.IsAssignableTo(typeof(DbContext)))
                                           .ToList();

        foreach (var descriptor in dbContextDescriptors)
        {
            services.Replace(new ServiceDescriptor(
                                                   descriptor.ServiceType,
                                                   provider => CreateWithFieldInjection(provider, descriptor),
                                                   descriptor.Lifetime));
        }

        return services;
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        return containerBuilder.BuildServiceProvider();
    }
    
    private static object CreateWithFieldInjection(IServiceProvider provider, ServiceDescriptor descriptor)
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

        InjectFields(provider, serviceInstance);

        return serviceInstance;
    }

    private static void InjectFields(IServiceProvider provider, object instance)
    {
        var serviceType = instance.GetType();

        if (!InjectableFields.TryGetValue(serviceType, out var fields))
        {
            fields = serviceType
                    .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    .Where(field => field.HasAttribute<InjectableAttribute>())
                    .ToList();

            InjectableFields.TryAdd(serviceType, fields);
        }

        if (fields.Count == 0)
        {
            return;
        }

        foreach (var field in fields)
        {
            var fieldType = field.FieldType;
            var service   = provider.GetService(fieldType);
            if (service != null)
            {
                field.SetValue(instance, service);
            }
        }
    }
}