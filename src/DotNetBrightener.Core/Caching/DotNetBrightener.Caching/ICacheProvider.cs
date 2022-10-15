namespace DotNetBrightener.Caching;

public interface ICacheProvider : IBaseCacheService
{
    bool CanUse { get; }
}