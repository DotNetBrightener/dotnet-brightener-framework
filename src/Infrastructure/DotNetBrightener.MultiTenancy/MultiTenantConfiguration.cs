using DotNetBrightener.DataAccess.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.MultiTenancy;

public class MultiTenantConfiguration(IServiceCollection serviceCollection)
{
    internal readonly IServiceCollection ServiceCollection = serviceCollection;

    internal static readonly HashSet<Type> TenantMappableEntityTypes = new();

    public MultiTenantConfiguration RegisterTenantMappableType<TType>() where TType : IBaseEntity
    {
        TenantMappableEntityTypes.Add(typeof(TType));

        return this;
    }

    public MultiTenantConfiguration RegisterTenantMappableType(Type type)
    {
        if (type.IsAssignableTo(typeof(IBaseEntity)))
            TenantMappableEntityTypes.Add(type);

        return this;
    }

    internal static bool ShouldIgnoreTenantMapping<T>()
    {
        return ShouldIgnoreTenantMapping(typeof(T));
    }

    internal static bool ShouldIgnoreTenantMapping(Type entityType)
    {
        return !typeof(IBaseEntity).IsAssignableFrom(entityType) ||
               !TenantMappableEntityTypes.Contains(entityType);
    }

    internal static string GetEntityType<TEntity>()
    {
        return GetEntityType(typeof(TEntity));
    }

    internal static string GetEntityType(Type entityType)
    {
        return entityType.FullName;
    }
}