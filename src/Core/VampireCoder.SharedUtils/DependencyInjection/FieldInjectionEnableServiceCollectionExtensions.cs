using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Concurrent;
using System.Reflection;

// ReSharper disable once CheckNamespace

#nullable enable
namespace VampireCoder.SharedUtils.DependencyInjection;

public static class FieldInjectionEnableServiceCollectionExtensions
{
    internal static readonly ConcurrentDictionary<Type, List<FieldInfo>> InjectableFields = new();

    public static void EnableFieldInjectionResolution(this IServiceCollection services)
    {
        foreach (var descriptor in services.ToList())
        {
            services.Replace(new ServiceDescriptor(
                                                   descriptor.ServiceType,
                                                   provider => CreateWithFieldInjection(provider, descriptor),
                                                   descriptor.Lifetime));
        }
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
            fields = serviceType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                .Where(field => field.GetCustomAttribute<InjectableAttribute>() != null)
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
            var service = provider.GetService(fieldType);
            if (service != null)
            {
                field.SetValue(instance, service);
            }
        }
    }
}