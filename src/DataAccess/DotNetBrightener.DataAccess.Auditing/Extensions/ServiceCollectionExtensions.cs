using DotNetBrightener.DataAccess.Auditing.Interceptors;
using DotNetBrightener.DataAccess.Auditing.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal class IgnoreAuditingEntitiesContainer : List<Type>;

public static class ServiceCollectionExtensions
{
    [Obsolete("Call AddAuditingService() method instead")]
    public static IServiceCollection AddAuditContext(this IServiceCollection services)
        => services.AddAuditingService();

    /// <summary>
    ///     Enables auditing for the application
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAuditingService(this IServiceCollection services)
    {
        services.TryAddScoped<IAuditEntriesContainer, AuditEntriesContainer>();
        services.TryAddSingleton<IgnoreAuditingEntitiesContainer>(new IgnoreAuditingEntitiesContainer());
        services.AddDbContextConfigurator<AuditEnabledDbContextConfigurator>();
        services.AddScoped<AuditEnabledSavingChangesInterceptor>();

        return services;
    }

    public static IServiceCollection IgnoreAuditingForTypes(this   IServiceCollection services,
                                                            params Type[]             ignoredTypes)
    {
        var ignoreAuditingEntitiesContainer =
            services.FirstOrDefault(x => x.ServiceType == typeof(IgnoreAuditingEntitiesContainer))
                   ?.ImplementationInstance as IgnoreAuditingEntitiesContainer;

        if (ignoreAuditingEntitiesContainer is null)
        {
            ignoreAuditingEntitiesContainer = new IgnoreAuditingEntitiesContainer();

            services.TryAddSingleton<IgnoreAuditingEntitiesContainer>(ignoreAuditingEntitiesContainer);
        }

        ignoreAuditingEntitiesContainer.AddRange(ignoredTypes);

        return services;
    }

    public static IServiceCollection IgnoreAuditingFor<TType>(this IServiceCollection services)
        where TType : class

    {
        var ignoreAuditingEntitiesContainer =
            services.FirstOrDefault(x => x.ServiceType == typeof(IgnoreAuditingEntitiesContainer))
                   ?.ImplementationInstance as IgnoreAuditingEntitiesContainer;

        if (ignoreAuditingEntitiesContainer is null)
        {
            ignoreAuditingEntitiesContainer = new IgnoreAuditingEntitiesContainer();

            services.TryAddSingleton<IgnoreAuditingEntitiesContainer>(ignoreAuditingEntitiesContainer);
        }

        ignoreAuditingEntitiesContainer.Add(typeof(TType));

        return services;
    }
}