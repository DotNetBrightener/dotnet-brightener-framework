namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public interface ICurrentRequestAudienceResolver
{
    string[] GetAudiences();
}