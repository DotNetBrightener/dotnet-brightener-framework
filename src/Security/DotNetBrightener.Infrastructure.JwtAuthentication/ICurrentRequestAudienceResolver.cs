namespace DotNetBrightener.Infrastructure.JwtAuthentication;

public interface ICurrentRequestAudienceResolver
{
    string[] GetAudiences();
}

class NullCurrentRequestAudienceResolver : ICurrentRequestAudienceResolver
{
    public string[] GetAudiences()
    {
        return [];
    }
}