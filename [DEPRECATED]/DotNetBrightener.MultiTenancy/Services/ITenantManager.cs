using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetBrightener.MultiTenancy.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DotNetBrightener.MultiTenancy.Services
{
    public interface ITenantManager
    {
        IEnumerable<ITenant> LoadTenants();

        ITenant SaveTenant(ITenant tenant);
        DateTimeOffset LastUpdatedTime { get; set; }
    }

    public class TenantManager : ITenantManager
    {
        public const       string              TenantSettingFileName  = "Site.json";
        protected readonly IConfiguration      Configuration;
        protected readonly IWebHostEnvironment HostingEnvironment;

        public DateTimeOffset LastUpdatedTime { get; set; }

        private readonly HashSet<ITenant> _allTenants      = new HashSet<ITenant>();
        private readonly HashSet<string>  _savingFileNames = new HashSet<string>();

        public TenantManager(IConfiguration      configuration,
                                    IWebHostEnvironment hostingEnvironment)
        {
            Configuration      = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IEnumerable<ITenant> LoadTenants()
        {
            if (_allTenants.Any())
                return _allTenants;

            _allTenants.Clear();
            var tenantSavingDirectory = GetTenantSavingDirectory();

            var tenantSettings = tenantSavingDirectory.GetFiles(TenantSettingFileName, SearchOption.AllDirectories)
                                                      .Select(tenantSettingFile =>
                                                                  File.ReadAllText(tenantSettingFile.FullName))
                                                      .Select(_ =>
                                                                  JsonConvert
                                                                     .DeserializeObject(_,
                                                                                        MultiTenantConfigurator
                                                                                           .TenantType))
                                                      .Cast<ITenant>()
                                                      .ToHashSet();

            if (!tenantSettings.Any())
            {
                return Enumerable.Empty<ITenant>();
            }

            _allTenants.UnionWith(tenantSettings);
            LastUpdatedTime = DateTimeOffset.Now;
            return _allTenants;
        }

        public ITenant SaveTenant(ITenant tenant)
        {
            var tenantSavingDirectory = GetTenantSavingDirectory();

            if (tenant.Id == 0)
            {
                tenant.Id = _allTenants.Any() ? _allTenants.Max(_ => _.Id) + 1 : 1;
            }


            var tenantFolder = Path.Combine(tenantSavingDirectory.FullName, tenant.Name);
            if (!Directory.Exists(tenantFolder))
            {
                Directory.CreateDirectory(tenantFolder);
            }

            var fileNameToSave = Path.Combine(tenantFolder, TenantSettingFileName);

            if (_savingFileNames.Contains(fileNameToSave))
            {
                // setting is being saved, ignore the saving
                return tenant;
            }

            _savingFileNames.Add(fileNameToSave);

            var settingToSave = JsonConvert.SerializeObject(tenant, Formatting.Indented);
            if (File.Exists(fileNameToSave))
            {
                var savedSettings = File.ReadAllText(fileNameToSave);
                if (savedSettings.Trim() == settingToSave.Trim())
                {
                    _savingFileNames.Remove(fileNameToSave);
                    return tenant; // saved data is same as data to be saved, skip saving;
                }
            }

            // write the setting to file
            File.WriteAllText(fileNameToSave, settingToSave);
            _savingFileNames.Remove(fileNameToSave);

            // update the tenants list
            _allTenants.RemoveWhere(_ => _.Name == tenant.Name);
            _allTenants.UnionWith(new[] { tenant });
            LastUpdatedTime = DateTimeOffset.Now;

            return tenant;
        }

        private DirectoryInfo GetTenantSavingDirectory()
        {
            var fullSavingPath = Path.Combine(HostingEnvironment.ContentRootPath, TenantHelpers.TenantSettingsSavePath);

            if (!Directory.Exists(fullSavingPath))
                Directory.CreateDirectory(fullSavingPath);

            var tenantSavingDirectory = new DirectoryInfo(fullSavingPath);
            return tenantSavingDirectory;
        }
    }
}