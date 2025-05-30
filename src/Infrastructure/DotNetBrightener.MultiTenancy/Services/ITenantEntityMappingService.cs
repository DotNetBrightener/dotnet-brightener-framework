using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.MultiTenancy.Entities;

namespace DotNetBrightener.MultiTenancy.Services;

public interface ITenantEntityMappingService
{
    IEnumerable<long> GetTenantMappingForEntity<T>(long entityId);
}

public class TenantEntityMappingService(IRepository repository) : ITenantEntityMappingService
{
    public IEnumerable<long> GetTenantMappingForEntity<T>(long entityId)
    {
        var entityName = MultiTenantConfiguration.GetEntityType<T>();

        var entityMappings =
            repository.Fetch<TenantEntityMapping, long>(m => m.EntityType == entityName &&
                                                             m.EntityId == entityId,
                                                        m => m.TenantId)
                      .ToArray();

        return entityMappings;
    }
}