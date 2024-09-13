using DotNetBrightener.MultiTenancy.Entities;
using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.MultiTenancy.Services;

public interface ITenantAccessor
{
    /// <summary>
    ///     Retrieves the current tenant ids of the current request context
    /// </summary>
    long[] CurrentTenantIds { get; }

    /// <summary>
    ///     Retrieves the tenant ids that are used to limit the records to be saved to
    /// </summary>
    long[] LimitedTenantIdsToRecordsPersistence { get; }

    /// <summary>
    ///     Temporarily specifies tenant ids for the following actions within the scope
    /// </summary>
    /// <param name="tenantIds">The tenant ids for using in the following actions</param>
    IDisposable UseTenant(long[] tenantIds);

    Tenant CurrentTenant { get; }
}

public class TenantAccessor(IHttpContextAccessor httpContextAccessor) : ITenantAccessor
{
    private readonly long[] _currentTenantIds =
        httpContextAccessor.RetrieveValue<long[]>(MultiTenantConstants.TenantIdsContextKey) ??
        [
        ];

    private readonly long[] _limitRecordsToTenantIds =
        httpContextAccessor.RetrieveValue<long[]>(MultiTenantConstants.LimitRecordToTenantIds) ??
        [
        ];

    private long[] _tempCurrentTenantIds;
    private long[] _tempLimitRecordsToTenantIds;

    public Tenant CurrentTenant => httpContextAccessor.RetrieveValue<Tenant>();

    public long[] CurrentTenantIds
    {
        get
        {
            if (_tempCurrentTenantIds != null)
                return _tempCurrentTenantIds;

            return _currentTenantIds ?? [];
        }
    }

    public long[] LimitedTenantIdsToRecordsPersistence
    {
        get
        {
            if (_tempLimitRecordsToTenantIds != null)
                return _tempLimitRecordsToTenantIds;

            return _limitRecordsToTenantIds ?? [];
        }
    }

    public IDisposable UseTenant(long[] tenantIds)
    {
        return new TenantScope(this, tenantIds);
    }

    private class TenantScope : IDisposable
    {
        private readonly TenantAccessor _tenantAccessor;

        public TenantScope(TenantAccessor tenantAccessor, long[] tenantIds)
        {
            _tenantAccessor = tenantAccessor;

            if (_tenantAccessor._tempCurrentTenantIds != null ||
                _tenantAccessor._tempLimitRecordsToTenantIds != null)
            {
                throw new
                    InvalidOperationException($"Cannot nested use tenant. Callback actions must be done within one tenant scope.");
            }

            _tenantAccessor._tempCurrentTenantIds        = tenantIds;
            _tenantAccessor._tempLimitRecordsToTenantIds = tenantIds;
        }

        public void Dispose()
        {
            _tenantAccessor._tempCurrentTenantIds        = null;
            _tenantAccessor._tempLimitRecordsToTenantIds = null;
        }
    }
}