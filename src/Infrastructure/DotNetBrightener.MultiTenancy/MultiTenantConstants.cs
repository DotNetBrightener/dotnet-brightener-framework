namespace DotNetBrightener.MultiTenancy;

public static class MultiTenantConstants
{
    internal const string TenantIdsContextKey = "CURRENT_TENANT_IDs";

    internal const string LimitRecordToTenantIds = "LIMIT_RECORD_TO_TENANT_IDs";

    /// <summary>
    ///     The header key to include in the request, which contains the information of which tenants to limit the record to
    /// </summary>
    /// <remarks>
    ///     Usage: {
    ///         "headers": {
    ///             "X_TENANT_LIMIT": "1, 2, 3"
    ///         }
    ///     }.<br />
    ///     If leave empty, the record will be available to all tenants
    /// </remarks>
    public const string LimitTenantIdsHeaderKey = "X_TENANT_LIMIT";

    public const string TenantIdentifierClaimKey = "TENANT_ID";
}