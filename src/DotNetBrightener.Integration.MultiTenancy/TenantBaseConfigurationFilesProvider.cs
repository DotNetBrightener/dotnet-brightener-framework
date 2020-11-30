using System.IO;
using DotNetBrightener.Core.IO;
using DotNetBrightener.MultiTenancy;
using DotNetBrightener.MultiTenancy.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace DotNetBrightener.Integration.MultiTenancy
{
    internal class TenantBaseConfigurationFilesProvider : PhysicalFileProvider, IConfigurationFilesProvider
    {
        private TenantBaseConfigurationFilesProvider(string root) : base(root)
        {
        }

        public static IConfigurationFilesProvider InitializeProvider(IWebHostEnvironment environment,
                                                                     ITenant             tenantSetting)
        {
            var rootPath = Path.Combine(environment.ContentRootPath,
                                        TenantHelpers.TenantSettingsSavePath,
                                        tenantSetting.Name);

            if (!Directory.Exists(rootPath))
                Directory.CreateDirectory(rootPath);

            return new TenantBaseConfigurationFilesProvider(rootPath);
        }
    }
}