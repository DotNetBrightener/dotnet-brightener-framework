using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.Extensions;

internal static class ServiceCollectionAutoRegisterDependenciesExtension
{
    public static object TryGetService<TService>(this IServiceProvider provider)
    {
        return TryGetService(provider, typeof(TService));
    }

    public static object TryGetService(this IServiceProvider provider, Type type)
    {
        Exception innerException = null;
        foreach (var constructor in type.GetConstructors())
        {
            try
            {
                //try to resolve constructor parameters
                var parameters = constructor.GetParameters()
                                            .Select(parameter =>
                                             {
                                                 var service = provider.GetService(parameter.ParameterType);
                                                 if (service == null)
                                                     throw new Exception("Unknown dependency");
                                                 return service;
                                             });

                //all is ok, so create instance
                return Activator.CreateInstance(type, parameters.ToArray());
            }
            catch (Exception ex)
            {
                innerException = ex;
            }
        }

        throw new Exception("No constructor was found that had all the dependencies satisfied.", innerException);
    }

    /// <summary>
    ///     Detects and registers the implementation services that implement <typeparamref name="TDependency"/> service type 
    /// </summary>
    /// <typeparam name="TDependency">The service type to register</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/></param>
    /// <param name="assembliesBundle">
    ///     All the assemblies loaded to the context
    /// </param>
    /// <param name="lifetime">
    ///     Specifies the <see cref="ServiceLifetime"/> for resolving the service
    /// </param>
    /// <param name="registerProvidedServiceType">
    ///     Indicates if the <typeparamref name="TDependency" /> should be registered as well.
    ///     It is only useful when the <typeparamref name="TDependency"/> is not directly implemented by the service
    /// </param>
    public static void RegisterServiceImplementations<TDependency>(this IServiceCollection serviceCollection,
                                                                   IEnumerable<Assembly>   assembliesBundle,
                                                                   ServiceLifetime lifetime =
                                                                       ServiceLifetime.Scoped,
                                                                   bool registerProvidedServiceType =
                                                                       false)
    {
        var serviceTypes = assembliesBundle.GetDerivedTypes<TDependency>();

        RegisterServiceImplementations<TDependency>(serviceCollection,
                                                    serviceTypes,
                                                    lifetime,
                                                    registerProvidedServiceType);
    }

    public static void RegisterImplementorsFromAssembly(this IServiceCollection serviceCollection,
                                                        Assembly                assembly)
    {
        var exportedTypes = assembly.GetExportedTypes()
                                    .Where(_ => !_.IsAbstract && !_.IsInterface);

        var lifetime = ServiceLifetime.Scoped;

        foreach (var dependencyType in exportedTypes)
        {
            var interfaces = dependencyType.GetInterfaces().ToArray();

            // Get only direct parent interfaces
            interfaces = interfaces.Except(interfaces.SelectMany(t => t.GetInterfaces()))
                                   .ToArray();

            if (interfaces.Any())
            {
                foreach (var @interface in interfaces)
                {
                    if (@interface.IsGenericType && dependencyType.IsGenericType)
                    {
                        var interfaceGenericTypeDef  = @interface.GetGenericTypeDefinition();
                        var dependencyGenericTypeDef = dependencyType.GetGenericTypeDefinition();

                        serviceCollection.Add(ServiceDescriptor.Describe(interfaceGenericTypeDef,
                                                                         dependencyGenericTypeDef,
                                                                         lifetime));
                        // register the type itself
                        serviceCollection.TryAdd(ServiceDescriptor.Describe(dependencyGenericTypeDef,
                                                                            dependencyGenericTypeDef,
                                                                            lifetime));
                    }
                    else
                    {
                        var existingRegistration = serviceCollection.FirstOrDefault(_ => _.ServiceType == @interface &&
                                                                                         _.ImplementationType ==
                                                                                         dependencyType);
                        if (existingRegistration != null)
                            continue;

                        serviceCollection.Add(ServiceDescriptor.Describe(@interface,
                                                                         dependencyType,
                                                                         lifetime));
                        // register the type itself
                        serviceCollection.TryAdd(ServiceDescriptor.Describe(dependencyType,
                                                                            dependencyType,
                                                                            lifetime));
                    }
                }
            }
        }
    }

    public static void ReplaceServiceLifetimeScope<TDependency>(this IServiceCollection serviceCollection,
                                                                ServiceLifetime         lifetime)
    {
        var serviceRegistrationsToReplace = serviceCollection.Where(_ => _.ServiceType == typeof(TDependency) &&
                                                                         _.Lifetime != lifetime)
                                                             .ToArray();

        foreach (var serviceRegistration in serviceRegistrationsToReplace)
        {
            ServiceDescriptor serviceDescriptor = null;

            if (serviceRegistration.ImplementationType != null)
            {
                serviceDescriptor = ServiceDescriptor.Describe(serviceRegistration.ServiceType,
                                                               serviceRegistration.ImplementationType,
                                                               lifetime);
            }

            if (serviceRegistration.ImplementationFactory != null)
            {
                serviceDescriptor = ServiceDescriptor.Describe(serviceRegistration.ServiceType,
                                                               serviceRegistration.ImplementationFactory,
                                                               lifetime);
            }

            if (serviceDescriptor != null)
                serviceCollection.Add(serviceDescriptor);
        }
    }

    /// <summary>
    /// Detects and registers the implementation services that implement <typeparamref name="TDependency"/> service type 
    /// </summary>
    /// <typeparam name="TDependency">The service type to register</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/></param>
    /// <param name="dependencyTypes">
    ///     All the detected service types loaded to the context
    /// </param>
    /// <param name="lifetime">
    ///     Specifies the <see cref="ServiceLifetime"/> for resolving the service
    /// </param>
    /// <param name="registerProvidedServiceType">
    ///     Indicates if the <typeparamref name="TDependency" /> should be registered as well.
    ///     It is only useful when the <typeparamref name="TDependency"/> is not directly implemented by the service
    /// </param>
    public static void RegisterServiceImplementations<TDependency>(this IServiceCollection serviceCollection,
                                                                   IEnumerable<Type>       dependencyTypes,
                                                                   ServiceLifetime lifetime =
                                                                       ServiceLifetime.Scoped,
                                                                   bool registerProvidedServiceType =
                                                                       false)
    {
        var typesToRegister = dependencyTypes.Where(_ => _.IsSubclassOf(typeof(TDependency)) ||
                                                         typeof(TDependency).IsAssignableFrom(_))
                                             .Distinct()
                                             .ToArray();

        if (!typesToRegister.Any())
            return;

        var shouldRegisterAsInterface = typeof(TDependency).IsInterface;

        foreach (var dependencyType in typesToRegister)
        {
            if (shouldRegisterAsInterface)
            {
                var interfaces = dependencyType.GetInterfaces().ToArray();

                // Get only direct parent interfaces
                interfaces = interfaces.Except(interfaces.SelectMany(t => t.GetInterfaces()))
                                       .ToArray();

                if (registerProvidedServiceType && !interfaces.Contains(typeof(TDependency)))
                {
                    interfaces = interfaces.Concat(new[] {typeof(TDependency)})
                                           .ToArray();
                }

                foreach (var @interface in interfaces)
                {
                    if (@interface.IsGenericType && dependencyType.IsGenericType)
                    {
                        var interfaceGenericTypeDef  = @interface.GetGenericTypeDefinition();
                        var dependencyGenericTypeDef = dependencyType.GetGenericTypeDefinition();

                        serviceCollection.Add(ServiceDescriptor.Describe(interfaceGenericTypeDef,
                                                                         dependencyGenericTypeDef,
                                                                         lifetime));
                        // register the type itself
                        serviceCollection.TryAdd(ServiceDescriptor.Describe(dependencyGenericTypeDef,
                                                                            dependencyGenericTypeDef,
                                                                            lifetime));
                    }
                    else
                    {
                        // find the existing registration
                        var existingRegistration = serviceCollection.FirstOrDefault(_ => _.ServiceType == @interface &&
                                                                                         _.ImplementationType ==
                                                                                         dependencyType);

                        if (existingRegistration != null)
                        {
                            // remove the existing registration if lifetime is different
                            if (existingRegistration.Lifetime != lifetime)
                            {
                                serviceCollection.Remove(existingRegistration);
                            }
                            else // already available, ignore for this service
                            {
                                continue;
                            }
                        }

                        // add correct service descriptor
                        serviceCollection.Add(ServiceDescriptor.Describe(@interface,
                                                                         dependencyType,
                                                                         lifetime));
                        // register the type itself
                        serviceCollection.TryAdd(ServiceDescriptor.Describe(dependencyType,
                                                                            dependencyType,
                                                                            lifetime));
                    }
                }
            }
            else
            {
                // register the type as the parent class
                if (dependencyType.BaseType != null)
                {
                    serviceCollection.Add(ServiceDescriptor.Describe(dependencyType.BaseType,
                                                                     dependencyType,
                                                                     lifetime));
                }

                if (registerProvidedServiceType)
                {
                    serviceCollection.Add(ServiceDescriptor.Describe(typeof(TDependency),
                                                                     dependencyType,
                                                                     lifetime));
                }

                // register the type itself
                serviceCollection.TryAdd(ServiceDescriptor.Describe(dependencyType,
                                                                    dependencyType,
                                                                    lifetime));
            }
        }
    }
}