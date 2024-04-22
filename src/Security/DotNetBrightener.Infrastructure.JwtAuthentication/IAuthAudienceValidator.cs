using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public interface IAuthAudienceValidator
{
    string[] GetValidAudiences();
}

public class DefaultAuthAudienceValidator : IAuthAudienceValidator
{
    private readonly IConfiguration _configuration;
    private readonly string?        _enableOrigins;

    public DefaultAuthAudienceValidator(IConfiguration configuration)
    {
        _configuration = configuration;
        _enableOrigins = _configuration.GetValue<string>(JwtAuthConstants.EnableOriginsConfigurationName);
    }

    public string[] GetValidAudiences()
    {
        if (string.IsNullOrEmpty(_enableOrigins))
            return [];

        var validAudiences = _enableOrigins.Split(new[]
                                                  {
                                                      ";", ","
                                                  },
                                                  StringSplitOptions.RemoveEmptyEntries);

        return validAudiences;
    }
}