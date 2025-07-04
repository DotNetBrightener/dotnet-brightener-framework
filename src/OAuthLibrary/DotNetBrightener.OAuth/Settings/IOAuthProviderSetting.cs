using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetBrightener.OAuth.Settings;

public interface IOAuthProviderSetting
{

}

public interface IOAuthProviderSettingLoader<TSetting>
    where TSetting : class, IOAuthProviderSetting
{
    Task<TSetting> LoadSettings();

    Task StoreSettings();
}

public class DefaultOAuthProviderSettingLoader<TSetting>(
    IOptions<TSetting>                                   settings,
    ILogger<DefaultOAuthProviderSettingLoader<TSetting>> logger)
    : IOAuthProviderSettingLoader<TSetting>
    where TSetting : class, IOAuthProviderSetting
{
    private readonly TSetting _configuration = settings.Value;
    private readonly ILogger  _logger        = logger;

    public Task<TSetting> LoadSettings()
    {
        return Task.FromResult(_configuration);
    }

    public Task StoreSettings()
    {
        _logger.LogWarning($"Default Loader for {typeof(TSetting).Name} Provider Setting does not support saving.");

        return Task.CompletedTask;
    }
}
