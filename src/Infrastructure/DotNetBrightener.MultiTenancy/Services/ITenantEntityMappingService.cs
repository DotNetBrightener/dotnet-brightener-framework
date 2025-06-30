using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.MultiTenancy.Entities;

namespace DotNetBrightener.MultiTenancy.Services;

public interface ITenantEntityMappingService
{
    Guid[] GetTenantMappingForEntity<T>(long entityId);
}

public class TenantEntityMappingService(IRepository repository) : ITenantEntityMappingService
{
    public Guid[] GetTenantMappingForEntity<T>(long entityId)
    {
        var entityName = MultiTenantConfiguration.GetEntityType<T>();

        var entityMappings =
            repository.Fetch<TenantEntityMapping, Guid>(m => m.EntityType == entityName &&
                                                             m.EntityId == entityId.ToString(),
                                                        m => m.TenantId)
                      .ToArray();

        return entityMappings;
    }
}