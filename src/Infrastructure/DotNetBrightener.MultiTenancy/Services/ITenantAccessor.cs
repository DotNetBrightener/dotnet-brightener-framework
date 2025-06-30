using DotNetBrightener.MultiTenancy.Entities;
using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.MultiTenancy.Services;

public interface ITenantAccessor
{
    /// <summary>
    ///     Retrieves the current tenant ids of the current request context
    /// </summary>
    Guid? CurrentTenantId { get; }

    /// <summary>
    ///     Retrieves the tenant ids that are used to limit the records to be saved to
    /// </summary>
    Guid[] LimitedTenantIdsToRecordsPersistence { get; }

    /// <summary>
    ///     Indicates that the operation should be done in all tenants
    /// </summary>
    bool IsFetchingAllTenants { get; }

    /// <summary>
    ///     Retrieves the current tenant, based on the current request
    /// </summary>
    TenantBase CurrentTenant { get; }

    /// <summary>
    ///     Temporarily specifies tenant id for the subsequent actions within the scope
    /// </summary>
    /// <param name="tenantId">
    ///     The tenant id for using in the subsequent actions
    /// </param>
    IDisposable UseTenant(Guid tenantId);

    /// <summary>
    ///     Indicates the fetch operation to load data in all tenants
    /// </summary>
    /// <returns></returns>
    IDisposable UseAllTenantFetchingScope();
}

public class TenantAccessor(IHttpContextAccessor httpContextAccessor) : ITenantAccessor
{
    private Guid[] LimitRecordsToTenantIds =>
        httpContextAccessor.RetrieveValue<Guid[]>(MultiTenantCacheKeys.LimitRecordToTenantIds);

    private Guid?  _tempCurrentTenantIds;
    private Guid[] _tempLimitRecordsToTenantIds = null;
    private bool   _isFetchingAllTenants        = false;

    public TenantBase CurrentTenant => httpContextAccessor.RetrieveValue<TenantBase>();

    public Guid? CurrentTenantId => _tempCurrentTenantIds ??
                                    httpContextAccessor.RetrieveValue<Guid>(MultiTenantCacheKeys.CurrentTenantId);

    public Guid[] LimitedTenantIdsToRecordsPersistence =>
        _tempLimitRecordsToTenantIds ?? LimitRecordsToTenantIds;

    public bool IsFetchingAllTenants => _isFetchingAllTenants;

    public IDisposable UseTenant(Guid tenantIds)
    {
        return new TenantScope(this, tenantIds);
    }

    public IDisposable UseAllTenantFetchingScope()
    {
        return new AllTenantsFetchingScope(this);
    }

    private class AllTenantsFetchingScope : IDisposable
    {
        private readonly TenantAccessor _tenantAccessor;

        public AllTenantsFetchingScope(TenantAccessor tenantAccessor)
        {
            _tenantAccessor                       = tenantAccessor;
            _tenantAccessor._isFetchingAllTenants = true;
        }

        public void Dispose()
        {
            _tenantAccessor._isFetchingAllTenants = false;
        }
    }

    private class TenantScope : IDisposable
    {
        private readonly TenantAccessor _tenantAccessor;

        public TenantScope(TenantAccessor tenantAccessor, Guid tenantIds)
        {
            _tenantAccessor = tenantAccessor;

            if (_tenantAccessor._tempCurrentTenantIds != null ||
                _tenantAccessor._tempLimitRecordsToTenantIds != null)
            {
                throw new
                    InvalidOperationException($"Cannot nested use tenant. Callback actions must be done within one tenant scope.");
            }

            _tenantAccessor._tempCurrentTenantIds        = tenantIds;
            _tenantAccessor._tempLimitRecordsToTenantIds = [tenantIds];
        }

        public void Dispose()
        {
            _tenantAccessor._tempCurrentTenantIds        = null;
            _tenantAccessor._tempLimitRecordsToTenantIds = null;
        }
    }
}