using System;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetBrightener.MultiTenancy;
using Newtonsoft.Json;

namespace DotNetBrightener.Integration.MultiTenancy
{
    /// <summary>
    /// Represents the tenant's settings
    /// </summary>
    public class TenantSetting: ITenant
    {
        public TenantSetting()
        {
            UseSeparateDbSettings = true;
            EnabledModules = new string[] { };
        }

        public TenantSetting(TenantSetting otherSetting)
        {
            Name = otherSetting.Name;
            Domains = otherSetting.Domains;
            DbConnectionString = otherSetting.DbConnectionString;
            EnabledModules = otherSetting.EnabledModules;
            UseSeparateDbSettings = otherSetting.UseSeparateDbSettings;
            IsActive = otherSetting.IsActive;
            TenantGuid = otherSetting.TenantGuid;
        }

        /// <summary>
        /// Unique Guid of the tenant
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Name of the tenant
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the domains of the current tenant
        /// </summary>
        public string Domains { get; set; }

        /// <summary>
        /// Specifies connection string to the database
        /// </summary>
        public string DbConnectionString { get; set; }

        /// <summary>
        /// Specifies the database provider
        /// </summary>
        public string DbProvider { get; set; }

        /// <summary>
        /// The unique Guid of the tenant
        /// </summary>
        public string TenantGuid { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Indicates whether Database settings should be separated from DefaultTenant
        /// </summary>
        public bool UseSeparateDbSettings { get; set; }

        [JsonIgnore] public string EnabledModulesString { get; set; }

        /// <summary>
        /// Specifies modules that are enabled.
        /// <para>Defines one element with "all" or "*" equivalents to load all modules</para>
        /// <para>Without "all" or "*", the system will only load the defined modules</para>
        /// <para>{</para>
        /// <para>    ...</para>
        /// <para>    "EnabledModules": [</para>
        /// <para>        "all",</para>
        /// <para>        "*",</para>
        /// <para>        or only</para>
        /// <para>        "your-module-name"</para>
        /// <para>    ],</para>
        /// <para>    ...</para>
        /// <para>}</para>
        /// </summary>
        [NotMapped]
        public string[] EnabledModules
        {
            get
            {
                return string.IsNullOrEmpty(EnabledModulesString)
                           ? new string[] { }
                           : JsonConvert.DeserializeObject<string[]>(EnabledModulesString);
            }
            set => EnabledModulesString = JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// Specifies whether the tenant is active or inactive
        /// </summary>
        public bool IsActive { get; set; }

        public void UpdateFrom(TenantSetting otherSetting)
        {
            Name = otherSetting.Name;
            Domains = otherSetting.Domains;
            DbConnectionString = otherSetting.DbConnectionString;
            EnabledModules = otherSetting.EnabledModules;
            UseSeparateDbSettings = otherSetting.UseSeparateDbSettings;
            IsActive = otherSetting.IsActive;
        }
    }
}