using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.MultiTenancy.Entities;

namespace DotNetBrightener.MultiTenancy.Services;

public interface ITenantEntityMappingService
{
    IEnumerable<long> GetTenantMappingForEntity<T>(long entityId);
}

public class TenantEntityMappingService : ITenantEntityMappingService
{
    private readonly IRepository _repository;

    public TenantEntityMappingService(IRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<long> GetTenantMappingForEntity<T>(long entityId)
    {
        var entityName = MultiTenantConfiguration.GetEntityType<T>();

        var entityMappings =
            _repository.Fetch<TenantEntityMapping, long>(_ => _.EntityType == entityName && _.EntityId == entityId,
                                                         _ => _.TenantId)
                       .ToArray();

        return entityMappings;
    }
}