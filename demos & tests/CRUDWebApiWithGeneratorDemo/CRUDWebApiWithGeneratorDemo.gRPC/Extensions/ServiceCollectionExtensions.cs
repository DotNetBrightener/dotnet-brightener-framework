using System.Reflection;
using DotNetBrightener;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CRUDWebApiWithGeneratorDemo.gRPC.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AutoRegisterDependencyServices(this IServiceCollection serviceCollection,
                                                      Assembly[] loadedAssemblies)
    {
        var dependencyTypes = loadedAssemblies.GetDerivedTypes<IDependency>();

        foreach (var dependencyType in dependencyTypes)
        {
            var interfaces = dependencyType.GetInterfaces().ToArray();

            // Get only direct parent interfaces
            interfaces = interfaces.Except(interfaces.SelectMany(t => t.GetInterfaces()))
                                   .ToArray();

            foreach (var @interface in interfaces)
            {
                var lifetime = ServiceLifetime.Scoped;

                if (@interface.IsAssignableTo(typeof(ISingletonDependency)))
                {
                    lifetime = ServiceLifetime.Singleton;
                }
                else if (@interface.IsAssignableTo(typeof(ITransientDependency)))
                {
                    lifetime = ServiceLifetime.Transient;
                }

                if (@interface.IsGenericType &&
                    dependencyType.IsGenericType)
                {
                    var interfaceGenericTypeDef = @interface.GetGenericTypeDefinition();
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
