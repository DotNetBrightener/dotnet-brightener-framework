using System;
using Microsoft.Extensions.Configuration;

namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public interface IAuthAudienceValidator
{
    void RegisterAudienceValidator(IAuthAudiencesContainer audiencesContainer);
}

public class DefaultAuthAudienceValidator : IAuthAudienceValidator
{
    private readonly IConfiguration _configuration;

    public DefaultAuthAudienceValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void RegisterAudienceValidator(IAuthAudiencesContainer audiencesContainer)
    {
        var enableOrigins = _configuration.GetValue<string>(JwtAuthConstants.EnableOriginsConfigurationName);

        if (string.IsNullOrEmpty(enableOrigins))
            return;

        audiencesContainer.RegisterValidAudience(enableOrigins.Split(new[]
                                                                     {
                                                                         ";", ","
                                                                     },
                                                                     StringSplitOptions.RemoveEmptyEntries));
    }
}