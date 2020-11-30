using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.MultiTenancy.Extensions
{
    public static class ServiceProviderExtensions
    {
        ///  <summary>
        /// 		Creates a child container.
        ///  </summary>
        ///  <param name="sourceCollection">The <see cref="IServiceCollection" /> contains the services to clone.</param>
        ///  <param name="serviceProvider">The root <see cref="IServiceCollection"/> that is used to resolve the services to put to the child collection</param>
        ///  <returns>
        /// 		The <see cref="IServiceProvider"/> which is the service container for the child scope
        ///  </returns>
        public static IServiceCollection CreateChildContainer(this IServiceCollection sourceCollection, IServiceProvider serviceProvider = null)
        {
            IServiceCollection clonedCollection = new ServiceCollection();
            var servicesByType = sourceCollection.GroupBy(s => s.ServiceType);

            serviceProvider ??= sourceCollection.BuildServiceProvider();

            foreach (var services in servicesByType)
            {
                // if only one service of a given type
                if (services.Count() == 1)
                {
                    var service = services.First();

                    // Register the singleton instances to all containers
                    if (service.Lifetime == ServiceLifetime.Singleton)
                    {
                        // Treat open-generic registrations differently
                        if (service.ServiceType.IsGenericType && service.ServiceType.GenericTypeArguments.Length == 0)
                        {
                            clonedCollection.Add(ServiceDescriptor.Describe(service.ServiceType,
                                                                            service.ImplementationType,
                                                                            service.Lifetime)
                                                );
                        }
                        else
                        {
                            // When a service from the main container is resolved, just add its instance to the container.
                            // It will be shared by all tenant service providers.
                            clonedCollection.AddSingleton(service.ServiceType, serviceProvider.GetService(service.ServiceType));
                        }
                    }
                    else
                    {
                        clonedCollection.Add(service);
                    }
                }

                // If multiple services of the same type
                else
                {
                    // If all services of the same type are not singletons.
                    if (services.All(s => s.Lifetime != ServiceLifetime.Singleton))
                    {
                        // We don't need to resolve them.
                        foreach (var service in services)
                        {
                            clonedCollection.Add(service);
                        }
                    }

                    // If all services of the same type are singletons.
                    else if (services.All(s => s.Lifetime == ServiceLifetime.Singleton))
                    {
                        // We can resolve them from the main container.
                        var instances = serviceProvider.GetServices(services.Key);

                        foreach (var instance in instances)
                        {
                            clonedCollection.AddSingleton(services.Key, instance);
                        }
                    }

                    // If singletons and scoped services are mixed.
                    else
                    {
                        // We need a service scope to resolve them.
                        using (var scope = serviceProvider.CreateScope())
                        {
                            var instances = scope.ServiceProvider
                                                 .GetServices(services.Key)
                                                 .ToImmutableArray();

                            // Then we only keep singleton instances.
                            for (var i = 0; i < services.Count(); i++)
                            {
                                if (services.ElementAt(i).Lifetime == ServiceLifetime.Singleton)
                                {
                                    clonedCollection.AddSingleton(services.Key, instances.ElementAt(i));
                                }
                                else
                                {
                                    clonedCollection.Add(services.ElementAt(i));
                                }
                            }
                        }
                    }
                }
            }

            return clonedCollection;
        }
    }
}