using DotNetBrightener.Caching;
using DotNetBrightener.OAuth.Models;
using System.Collections.Concurrent;

namespace DotNetBrightener.OAuth.Services;

public interface IOAuthRequestManager
{
    void CacheOAuthRequest(OAuthRequestModel model);

    OAuthRequestModel GetOAuthRequestAndRemove(string requestId);

    string CacheAuthenticatedOAuthResponse(OAuthUser model);

    OAuthUser RetrieveCachedOAuthUserResponse(string cacheId);
}

public class OAuthRequestManager : IOAuthRequestManager
{
    private readonly IDictionary<string, OAuthRequestModel> _requests =
        new ConcurrentDictionary<string, OAuthRequestModel>();

    private readonly IDictionary<string, OAuthUser> _oAuthUserResponses =
        new ConcurrentDictionary<string, OAuthUser>();

    private readonly ICacheManager _cacheManager;

    public OAuthRequestManager(IServiceProvider serviceProvider)
    {
        var cacheManager = serviceProvider.TryGetService<ICacheManager>();
        if (cacheManager is not null)
            _cacheManager = cacheManager;
    }

    public void CacheOAuthRequest(OAuthRequestModel model)
    {
        if (_cacheManager is not null)
        {
            _cacheManager.Set(new CacheKey(model.RequestId), model);

            return;
        }

        _requests.TryAdd(model.RequestId, model);
    }

    public OAuthRequestModel GetOAuthRequestAndRemove(string requestId)
    {
        if (_cacheManager is not null)
        {
            var cacheKey      = new CacheKey(requestId);
            var cachedRequest = _cacheManager.Get<OAuthRequestModel>(cacheKey, () => null);

            if (cachedRequest is not null)
            {
                _cacheManager.Remove(cacheKey);

                return cachedRequest;
            }
        }

        _requests.TryGetValue(requestId, out var request);

        if (request != null)
            _requests.Remove(requestId);

        return request;
    }

    public string CacheAuthenticatedOAuthResponse(OAuthUser model)
    {
        var cacheKey = $"OAUTH_USER_{Guid.NewGuid().ToString().Replace("-", "")}_{DateTime.Now:yyyyMMddHHmmss}";

        if (_cacheManager is not null)
        {
            _cacheManager.Set(new CacheKey(cacheKey), model);
        }
        else
        {
            _oAuthUserResponses.TryAdd(cacheKey, model);
        }

        return cacheKey;
    }

    public OAuthUser RetrieveCachedOAuthUserResponse(string cacheId)
    {
        if (_cacheManager is not null)
        {
            var cacheKey      = new CacheKey(cacheId);
            var cachedRequest = _cacheManager.Get<OAuthUser>(cacheKey, () => null);

            if (cachedRequest is not null)
            {
                _cacheManager.Remove(cacheKey);

                return cachedRequest;
            }
        }

        _oAuthUserResponses.TryGetValue(cacheId, out var oAuthUser);

        return oAuthUser;
    }
}