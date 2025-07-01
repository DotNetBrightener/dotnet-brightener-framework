namespace DotNetBrightener.MultiTenancy;

internal static class MultiTenantCacheKeys
{
    /// <summary>
    ///     The key to identify the identifier of the current tenant of the request
    /// </summary>
    internal const string CurrentTenantId = nameof(CurrentTenantId);

    /// <summary>
    ///     The key to identify the ids of tenants to limit saving records to
    /// </summary>
    internal const string LimitRecordToTenantIds = nameof(LimitRecordToTenantIds);
}

public static class MultiTenantHeaders
{

    /// <summary>
    ///     The key of header included in the request,
    ///     which contains the information of which tenants to limit the record to
    /// </summary>
    /// <remarks>
    ///     Usage: {
    ///         "headers": {
    ///             "X_TENANT_LIMIT": "1, 2, 3"
    ///         }
    ///     }.<br />
    ///     If leave empty, the record will be available to all tenants
    /// </remarks>
    public const string LimitTenantIds = "X_TENANTS_LIMIT";

    /// <summary>
    ///     The key of claim to include in user's identity,
    ///     to identify which tenant context user currently in
    /// </summary>
    public const string CurrentTenantId = "CURRENT_TENANT_ID";
}