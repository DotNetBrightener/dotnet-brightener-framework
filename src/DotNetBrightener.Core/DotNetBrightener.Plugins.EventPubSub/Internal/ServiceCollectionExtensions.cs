using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.Extensions
{
    internal static partial class ServiceCollectionExtensions
    {
        public static Type[] GetDerivedTypes<TParent>(this IEnumerable<Assembly> assemblies)
        {
            return GetDerivedTypes(assemblies, typeof(TParent)).ToArray();
        }

        public static Type[] GetDerivedTypes<TParent>(this Assembly assembly)
        {
            return GetDerivedTypes(new[] { assembly }, typeof(TParent)).ToArray();
        }

        private static List<Type> GetDerivedTypes(this IEnumerable<Assembly> bundledAssemblies, Type t)
        {
            var result = new List<Type>();

            foreach (var assembly in bundledAssemblies)
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }

                foreach (Type type in types)
                {
                    try
                    {
                        if ((t.IsAssignableFrom(type) ||
                             type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == t)) &&
                            !type.IsAbstract && !type.IsInterface)
                        {
                            result.Add(type);
                        }
                    }
                    catch (Exception err)
                    {
                    }
                }
            }

            return result;
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
                                                                       IEnumerable<Assembly> assembliesBundle,
                                                                       ServiceLifetime lifetime = ServiceLifetime.Scoped,
                                                                       bool registerProvidedServiceType = false)
        {
            var serviceTypes = assembliesBundle.GetDerivedTypes<TDependency>();

            RegisterServiceImplementations<TDependency>(serviceCollection,
                                                        serviceTypes,
                                                        lifetime,
                                                        registerProvidedServiceType);
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
                                                                       IEnumerable<Type> dependencyTypes,
                                                                       ServiceLifetime lifetime = ServiceLifetime.Scoped,
                                                                       bool registerProvidedServiceType = false)
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
                        interfaces = interfaces.Concat(new[] { typeof(TDependency) })
                                               .ToArray();
                    }

                    foreach (var @interface in interfaces)
                    {
                        if (@interface.IsGenericType && dependencyType.IsGenericType)
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
}
