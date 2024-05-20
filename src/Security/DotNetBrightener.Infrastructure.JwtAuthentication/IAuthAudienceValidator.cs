namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public interface IAuthAudienceValidator
{
    string[] GetValidAudiences();
}