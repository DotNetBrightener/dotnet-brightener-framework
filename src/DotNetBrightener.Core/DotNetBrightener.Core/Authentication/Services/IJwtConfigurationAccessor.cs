using DotNetBrightener.Core.Authentication.Configs;

namespace DotNetBrightener.Core.Authentication.Services;

public interface IJwtConfigurationAccessor
{
    JwtConfig RetrieveConfig(string kid = JwtConfig.DefaultJwtKId);
}