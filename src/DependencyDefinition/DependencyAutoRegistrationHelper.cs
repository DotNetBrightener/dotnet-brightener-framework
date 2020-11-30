using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyDefinition
{
    public static class DependencyAutoRegistrationHelper
    {
        public static void AutoRegisterDependencyServices(this IServiceCollection services,
                                                          Type[]                  serviceTypes)
        {

            var selfRegisterType = serviceTypes
                                  .Where(type => !type.IsGenericType)
                                  .Where(
                                         type =>
                                             type.GetInterfaces()
                                                 .Any(
                                                      intf =>
                                                          intf.Name.Equals(nameof(ISingletonSelfRegisterDependency)) ||
                                                          intf.Name.Equals(nameof(ISelfRegisterDependency))));

            services
               .RegisterDependencyClasses<ISelfRegisterDependency, ISingletonSelfRegisterDependency>(selfRegisterType);

            var dependencyTypes = serviceTypes
               .Where(
                      type =>
                          type.GetInterfaces()
                              .Any(intf => intf.Name.Equals(nameof(IDependency)) ||
                                           intf.Name.Equals(nameof(ISingletonDependency))));

            RegisterDependencyServices<IDependency, ISingletonDependency>(services, dependencyTypes);
        }

        public static void RegisterDependencyClasses<TDependency, TSingletonDependency>(
            this IServiceCollection service,
            IEnumerable<Type>       selfRegisterType)
        {
            foreach (var dependencyType in selfRegisterType)
            {
                var shouldRegisterAsSingleton = dependencyType.GetInterfaces()
                                                              .Any(x => x.Name.Equals(typeof(TSingletonDependency)
                                                                                         .Name));

                if (shouldRegisterAsSingleton)
                {
                    service.AddSingleton(dependencyType);
                }
                else
                {
                    service.AddScoped(dependencyType);
                }
            }
        }

        public static void RegisterDependencyServices<TDependency, TSingletonDependency>(
            this IServiceCollection service, IEnumerable<Type> dependencyTypes)
        {
            var singletonDependencyName = typeof(TSingletonDependency).Name;

            foreach (var dependencyType in dependencyTypes)
            {
                var interfaces = dependencyType.GetInterfaces()
                                               .Where(
                                                      x =>
                                                          !x.Name.Equals(typeof(TDependency).Name) &&
                                                          !x.Name.Equals(singletonDependencyName))
                                               .ToArray();

                // Get only direct parent interfaces
                interfaces = interfaces.Except(interfaces.SelectMany(t => t.GetInterfaces())).ToArray();

                var shouldRegisterAsSingleton = interfaces.SelectMany(x => x.GetInterfaces())
                                                          .Any(itf => itf.Name.Equals(singletonDependencyName));

                foreach (var _interface in interfaces)
                {
                    if (_interface.IsGenericType && dependencyType.IsGenericType)
                    {
                        if (shouldRegisterAsSingleton)
                        {
                            service.AddSingleton(_interface.GetGenericTypeDefinition(),
                                                 dependencyType.GetGenericTypeDefinition());
                            service.AddSingleton(dependencyType.GetGenericTypeDefinition());
                        }
                        else
                        {
                            service.AddScoped(_interface.GetGenericTypeDefinition(),
                                              dependencyType.GetGenericTypeDefinition());
                            service.AddScoped(dependencyType.GetGenericTypeDefinition());
                        }
                    }
                    else
                    {
                        if (shouldRegisterAsSingleton)
                        {
                            service.AddSingleton(_interface, dependencyType);
                            service.AddSingleton(dependencyType);
                        }
                        else
                        {
                            service.AddScoped(_interface, dependencyType);
                            service.AddScoped(dependencyType);
                        }
                    }
                }
            }
        }
    }
}