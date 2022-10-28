using System;
using System.Collections.Generic;
using DotNetBrightener.DataAccess.Models;

namespace DotNetBrightener.MultiTenancy;

public class MultiTenantConfiguration
{
    internal static readonly HashSet<Type> TenantMappableEntityTypes = new();

    public void RegisterTenantMappableType<TType>() where TType : BaseEntity
    {
        TenantMappableEntityTypes.Add(typeof(TType));
    }

    public void RegisterTenantMappableType(Type type)
    {
        if (type.IsAssignableTo(typeof(BaseEntity)))
            TenantMappableEntityTypes.Add(type);
    }

    internal static bool ShouldIgnoreTenantMapping<T>()
    {
        return ShouldIgnoreTenantMapping(typeof(T));
    }

    internal static bool ShouldIgnoreTenantMapping(Type entityType)
    {
        return !typeof(BaseEntity).IsAssignableFrom(entityType) ||
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