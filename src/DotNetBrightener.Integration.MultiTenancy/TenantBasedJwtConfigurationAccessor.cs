using System.Collections.Concurrent;
using System.IO;
using DotNetBrightener.Core.Authentication.Configs;
using DotNetBrightener.Core.Authentication.Services;
using DotNetBrightener.Core.Encryption;
using DotNetBrightener.MultiTenancy.Extensions;
using DotNetBrightener.MultiTenancy.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace DotNetBrightener.Integration.MultiTenancy
{
    public class TenantBasedJwtConfigurationAccessor : IJwtConfigurationAccessor
    {
        private const    string               JwtConfigurationFileName = "JwtConfig.json";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRunningTenantTable  _runningTenantTable;
        private readonly IWebHostEnvironment  _webHostEnvironment;

        private static readonly ConcurrentDictionary<string, JwtConfig> CachedJwtConfigs =
            new ConcurrentDictionary<string, JwtConfig>();

        public TenantBasedJwtConfigurationAccessor(IWebHostEnvironment  webHostEnvironment,
                                                   IRunningTenantTable  runningTenantTable,
                                                   IHttpContextAccessor httpContextAccessor)
        {
            _webHostEnvironment  = webHostEnvironment;
            _runningTenantTable  = runningTenantTable;
            _httpContextAccessor = httpContextAccessor;
        }

        public JwtConfig RetrieveConfig(string kid = JwtConfig.DefaultJwtKId)
        {
            var tenantSetting = _runningTenantTable.DetectTenantFromContext(_httpContextAccessor.HttpContext);
            if (tenantSetting == null)
                return null;

            if (tenantSetting.TenantGuid != kid)
                return null;

            if (CachedJwtConfigs.TryGetValue(kid, out var config))
                return config;

            using var configurationFilesProvider =
                new PhysicalFileProvider(Path.Combine(_webHostEnvironment.ContentRootPath,
                                                      TenantHelpers.TenantSettingsSavePath,
                                                      tenantSetting.Name));

            var       fileInfo = configurationFilesProvider.GetFileInfo(JwtConfigurationFileName);
            JwtConfig jwtTokenOptions;


            if (!fileInfo.Exists)
            {
                var audiences = JwtConfig.JwtDefaultIssuer;

                if (!string.IsNullOrEmpty(tenantSetting.Domains))
                {
                    audiences = tenantSetting.Domains;
                }

                jwtTokenOptions = new JwtConfig
                {
                    SigningKey = CryptoUtilities.GenerateRandomString(64),
                    Audience   = audiences,
                    Issuer     = JwtConfig.JwtDefaultIssuer
                };

                File.WriteAllText(fileInfo.PhysicalPath,
                                  JsonConvert.SerializeObject(jwtTokenOptions, Formatting.Indented));
            }
            else
            {
                jwtTokenOptions = JsonConvert.DeserializeObject<JwtConfig>(File.ReadAllText(fileInfo.PhysicalPath));
            }

            jwtTokenOptions.KID = kid;
            CachedJwtConfigs.TryAdd(kid, jwtTokenOptions);
            return jwtTokenOptions;
        }
    }
}