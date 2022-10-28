using DotNetBrightener.MultiTenancy.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

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
    ///     Temporarily specifies tenant ids for given callback action
    /// </summary>
    /// <param name="tenantIds">The tenant ids for using in the callback action</param>
    /// <param name="callback">The callback action</param>
    /// <returns></returns>
    Task UseTenant(long[] tenantIds, Action callback);

    Tenant CurrentTenant { get; }
}

public class TenantAccessor : ITenantAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly long[]               _currentTenantIds;
    private readonly long[]               _limitRecordsToTenantIds;
    private          long[]               _tempCurrentTenantIds;
    private          long[]               _tempLimitRecordsToTenantIds;

    public Tenant CurrentTenant => _httpContextAccessor.RetrieveValue<Tenant>();

    public long[] CurrentTenantIds
    {
        get
        {
            if (_tempCurrentTenantIds != null)
                return _tempCurrentTenantIds;

            return _currentTenantIds ?? Array.Empty<long>();
        }
    }

    public long[] LimitedTenantIdsToRecordsPersistence
    {
        get
        {
            if (_tempLimitRecordsToTenantIds != null)
                return _tempLimitRecordsToTenantIds;

            return _limitRecordsToTenantIds ?? Array.Empty<long>();
        }
    }

    public TenantAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _currentTenantIds = httpContextAccessor.RetrieveValue<long[]>(MultiTenantConstants.TenantIdsContextKey) ??
                            new long[]
                            {
                            };

        _limitRecordsToTenantIds =
            httpContextAccessor.RetrieveValue<long[]>(MultiTenantConstants.LimitRecordToTenantIds) ??
            new long[]
            {
            };
    }

    public async Task UseTenant(long[] tenantIds, Action callback)
    {
        if (_tempCurrentTenantIds != null ||
            _tempLimitRecordsToTenantIds != null)
        {
            throw new
                InvalidOperationException($"Cannot nested use tenant. Callback actions must be done within one tenant scope.");
        }

        _tempCurrentTenantIds        = tenantIds;
        _tempLimitRecordsToTenantIds = tenantIds;

        await Task.Run(callback);

        _tempCurrentTenantIds        = null;
        _tempLimitRecordsToTenantIds = null;
    }
}