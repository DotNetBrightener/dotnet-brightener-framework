using System.IO;
using DotNetBrightener.Core.Authentication.Configs;
using DotNetBrightener.Core.Encryption;
using DotNetBrightener.Core.IO;
using Newtonsoft.Json;

namespace DotNetBrightener.Core.Authentication.Services
{
    public interface IJwtConfigurationAccessor
    {
        JwtConfig RetrieveConfig(string kid = JwtConfig.DefaultJwtKId);
    }

    public class JwtConfigurationAccessor : IJwtConfigurationAccessor
    {
        private static   JwtConfig                   _sharedConfig;
        private const    string                      JwtConfigurationFileName = "JwtConfig.json";
        private readonly IConfigurationFilesProvider _configurationFilesProvider;

        public JwtConfigurationAccessor(IConfigurationFilesProvider configurationFilesProvider)
        {
            _configurationFilesProvider = configurationFilesProvider;
        }

        public JwtConfig RetrieveConfig(string kid = JwtConfig.DefaultJwtKId)
        {
            if (_sharedConfig != null)
                return _sharedConfig;

            var       fileInfo = _configurationFilesProvider.GetFileInfo(JwtConfigurationFileName);
            JwtConfig jwtTokenOptions;

            if (!fileInfo.Exists)
            {
                jwtTokenOptions = new JwtConfig
                {
                    SigningKey = CryptoUtilities.GenerateRandomString(64),
                    Audience   = JwtConfig.JwtDefaultIssuer,
                    Issuer     = JwtConfig.JwtDefaultIssuer
                };

                File.WriteAllText(fileInfo.PhysicalPath,
                                  JsonConvert.SerializeObject(jwtTokenOptions, Formatting.Indented, CoreConstants.DefaultJsonSerializerSettings));
            }
            else
            {
                jwtTokenOptions = JsonConvert.DeserializeObject<JwtConfig>(File.ReadAllText(fileInfo.PhysicalPath));
            }

            return _sharedConfig = jwtTokenOptions;
        }
    }
}