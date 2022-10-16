using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceCollectionLazySupportExtensions
{
    public static IServiceCollection EnableLazyResolver(this IServiceCollection serviceCollection)
    {
        var servicesByType = serviceCollection.GroupBy(s => s.ServiceType);

        foreach (var services in servicesByType)
        {
            var service = services.First();

            // if only one service of a given type
            if (services.Count() == 1)
            {
                var lazyType = typeof(Lazy<>).MakeGenericType(service.ServiceType);

                var existingService = serviceCollection.FirstOrDefault(_ => _.ServiceType == lazyType);
                if (existingService != null)
                {
                    serviceCollection.Remove(existingService);
                }

                var internalLazyType = typeof(InternalLazy<>).MakeGenericType(service.ServiceType);
                serviceCollection.Add(ServiceDescriptor.Scoped(lazyType,
                                                               provider => Activator.CreateInstance(internalLazyType, provider)));
            }

            // If multiple services of the same type
            else
            {
                var enumerableOfType = typeof(IEnumerable<>).MakeGenericType(service.ServiceType);
                var lazyType         = typeof(Lazy<>).MakeGenericType(enumerableOfType);

                var existingService = serviceCollection.FirstOrDefault(_ => _.ServiceType == lazyType);
                if (existingService != null)
                {
                    serviceCollection.Remove(existingService);
                }

                var internalLazyType = typeof(InternalEnumerableLazy<>).MakeGenericType(service.ServiceType);

                serviceCollection.Add(ServiceDescriptor.Scoped(lazyType,
                                                               provider => Activator.CreateInstance(internalLazyType, provider)));
            }
        }

        return serviceCollection;
    }

    internal class InternalLazy<T> : Lazy<T>
    {
        public InternalLazy(IServiceProvider serviceProvider)
            : base(() => serviceProvider.GetService<T>())
        {

        }
    }

    internal class InternalEnumerableLazy<T> : Lazy<IEnumerable<T>>
    {
        public InternalEnumerableLazy(IServiceProvider serviceProvider)
            : base(() => serviceProvider.GetServices<T>())
        {

        }
    }
}