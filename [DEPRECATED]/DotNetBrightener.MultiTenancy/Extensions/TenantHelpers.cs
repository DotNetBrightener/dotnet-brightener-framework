using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetBrightener.MultiTenancy.Extensions
{
    public static class TenantHelpers
    {
        public const string MasterTenantName = "Master";
        public const string TenantSettingsSavePath = "Tenants";

        public static ITenant DefaultUninitializedTenant;

        public static string[] ParseHostValues(this ITenant tenant)
        {
            if (tenant == null)
                throw new ArgumentNullException(nameof(tenant));

            var parsedValues = new List<string>();
            if (String.IsNullOrEmpty(tenant.Domains))
                return new string[] { };

            var hosts = tenant.Domains.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries);
            parsedValues.AddRange(hosts.Select(host => host.Trim())
                                       .Where(tmp => !String.IsNullOrEmpty(tmp)));

            return parsedValues.ToArray();
        }

        public static bool ContainsHostValue(this ITenant tenant, string host)
        {
            if (tenant == null)
                throw new ArgumentNullException(nameof(tenant));

            if (String.IsNullOrEmpty(host))
                return false;

            var contains = ParseHostValues(tenant).Any(x => x.Equals(host, StringComparison.InvariantCultureIgnoreCase));

            return contains;
        }
    }
}