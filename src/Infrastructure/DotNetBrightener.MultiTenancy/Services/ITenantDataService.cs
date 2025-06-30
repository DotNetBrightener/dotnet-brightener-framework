using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.MultiTenancy.Entities;

namespace DotNetBrightener.MultiTenancy.Services;

public interface ITenantDataService<TTenantEntity> : IBaseDataService<TTenantEntity> where TTenantEntity: TenantBase, new()
{
    TTenantEntity? GetTenantByDomain(string hostname);
}

public class TenantDataService<TTenantEntity>(IRepository repository)
    : BaseDataService<TTenantEntity>(repository), ITenantDataService<TTenantEntity> where TTenantEntity : TenantBase, new()
{
    public TTenantEntity? GetTenantByDomain(string hostname)
    {
        if (TenantSupportedRepository.HasTenantMapping == false)
            return null;

        var hostnameCondition = hostname + ";";

        return Get(t => t.TenantDomains != null && t.TenantDomains.Contains(hostnameCondition));
    }
}