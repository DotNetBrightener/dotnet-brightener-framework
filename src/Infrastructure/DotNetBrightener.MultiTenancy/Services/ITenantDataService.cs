using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.MultiTenancy.Entities;

namespace DotNetBrightener.MultiTenancy.Services;

public interface ITenantDataService : IBaseDataService<Tenant>
{
    Tenant GetTenantByHostName(string hostname);
}

public class TenantDataService : BaseDataService<Tenant>, ITenantDataService
{
    public TenantDataService(IRepository repository)
        : base(repository)
    {
    }

    public Tenant GetTenantByHostName(string hostname)
    {
        if (TenantSupportedRepository.HasTenantMapping == false)
            return null;

        var hostnameCondition = hostname + ";";

        return Get(_ => _.TenantDomains.Contains(hostnameCondition));
    }
}