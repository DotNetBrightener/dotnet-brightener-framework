using DotNetBrightener.DataAccess.EF.Auditing;
using DotNetBrightener.DataAccess.EF.Auditing.Internal;
using DotNetBrightener.DataAccess.EF.Interceptors;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal class IgnoreAuditingEntitiesContainer : List<Type>;

public static class AuditingEnableServiceCollectionExtensions
{
    /// <summary>
    ///     Enables auditing for the application
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAuditingService(this IServiceCollection services)
    {
        var ignoreAuditingEntitiesContainer = new IgnoreAuditingEntitiesContainer();

        services.TryAddSingleton<IgnoreAuditingEntitiesContainer>(ignoreAuditingEntitiesContainer);
        services.TryAddSingleton<IAuditEntriesProcessor, AuditEntriesProcessor>();

        services.TryAddScoped<IAuditEntriesContainer, AuditEntriesContainer>();
        services.AddDbContextConfigurator<AuditEnabledDbContextConfigurator>();
        services.TryAddScoped<AuditEnabledSavingChangesInterceptor>();

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
            ignoreAuditingEntitiesContainer = [];

            services.TryAddSingleton(ignoreAuditingEntitiesContainer);
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
            ignoreAuditingEntitiesContainer = [];

            services.TryAddSingleton(ignoreAuditingEntitiesContainer);
        }

        ignoreAuditingEntitiesContainer.Add(typeof(TType));

        return services;
    }
}