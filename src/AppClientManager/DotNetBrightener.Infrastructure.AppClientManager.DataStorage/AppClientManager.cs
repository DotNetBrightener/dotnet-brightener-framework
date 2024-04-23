using DotNetBrightener.Infrastructure.AppClientManager.Models;
using DotNetBrightener.Infrastructure.AppClientManager.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage;

internal class AppClientManager : IAppClientManager
{
    private readonly IAppClientRepository _appClientRepository;
    private readonly IMemoryCache         _memoryCache;
    private readonly ILogger              _logger;

    public AppClientManager(IAppClientRepository      appClientRepository,
                            ILogger<AppClientManager> logger,
                            IMemoryCache              memoryCache)
    {
        _logger              = logger;
        _memoryCache         = memoryCache;
        _appClientRepository = appClientRepository;
    }

    public virtual async Task CreateAppClient(AppClient appClient)
    {
        var (allowedOrigins, allowedAppBundleIds, origins, bundleIds) = EnsureOriginsAndBundleIds(appClient);

        var appClientEntity = new AppClient
        {
            ClientId            = appClient.ClientId,
            ClientName          = appClient.ClientName,
            AllowedOrigins      = allowedOrigins,
            AllowedAppBundleIds = allowedAppBundleIds,
            ClientType          = appClient.ClientType,
            ClientStatus        = appClient.ClientStatus
        };

        _appClientRepository.Insert(appClientEntity);
        _appClientRepository.CommitChanges();
    }

    public virtual async Task UpdateAppClient(AppClient appClient)
    {
        var (allowedOrigins, allowedAppBundleIds, origins, bundleIds) = EnsureOriginsAndBundleIds(appClient);

        _appClientRepository.Update<AppClient>(_ => _.Id == appClient.Id,
                                               _ => new AppClient
                                               {
                                                   ClientId            = appClient.ClientId,
                                                   ClientName          = appClient.ClientName,
                                                   AllowedOrigins      = allowedOrigins,
                                                   AllowedAppBundleIds = allowedAppBundleIds,
                                                   ClientType          = appClient.ClientType,
                                                   ClientStatus        = appClient.ClientStatus
                                               });
        _appClientRepository.CommitChanges();
    }

    public List<AppClient> GetAllAppClients()
    {
        var appClient = _appClientRepository.Fetch<AppClient>();

        return appClient.ToList();
    }

    public virtual async Task<AppClient?> GetClientByHostNameOrByBundleId(string hostNameOrBundleId)
    {
        if (_memoryCache.TryGetValue(CacheKeyByHostNameOrBundleId(hostNameOrBundleId), out var client) &&
            client is AppClient appClient)
            return appClient;

        var condition = hostNameOrBundleId + ";";

        appClient =
            _appClientRepository.Get<AppClient>(ac => ac.AllowedOrigins == hostNameOrBundleId ||
                                                      ac.AllowedOrigins.Contains(condition));

        if (appClient != null)
        {
            _memoryCache.Set(CacheKeyByHostNameOrBundleId(hostNameOrBundleId), appClient);

            return appClient;
        }

        appClient = _appClientRepository.Get<AppClient>(ac => ac.AllowedAppBundleIds == hostNameOrBundleId ||
                                                              ac.AllowedAppBundleIds.Contains(condition));

        if (appClient != null)
        {
            _memoryCache.Set(CacheKeyByHostNameOrBundleId(hostNameOrBundleId), appClient);

            return appClient;
        }

        return null;
    }

    public virtual async Task<AppClient?> GetClientByClientId(string clientId)
    {
        if (_memoryCache.TryGetValue(CacheKeyByClientId(clientId), out var client) &&
            client is AppClient appClient)
            return appClient;

        appClient = _appClientRepository.Get<AppClient>(ac => ac.ClientId == clientId);

        if (appClient != null)
            _memoryCache.Set(CacheKeyByClientId(clientId), appClient);

        return appClient;
    }


    private static AppClientData EnsureOriginsAndBundleIds(AppClient appClient)
    {
        var          allowedOrigins      = "";
        var          allowedAppBundleIds = "";
        List<string> origins             = [];
        List<string> bundleIds           = [];

        if (!string.IsNullOrEmpty(appClient.AllowedOrigins))
        {
            origins = appClient.AllowedOrigins
                               .Split([";", ","], StringSplitOptions.RemoveEmptyEntries)
                               .ToList();

            allowedOrigins = string.Join(";", origins) + ";"; // make sure to append the last semicolon
        }

        if (!string.IsNullOrEmpty(appClient.AllowedAppBundleIds))
        {
            bundleIds = appClient.AllowedAppBundleIds
                                 .Split([";", ","], StringSplitOptions.RemoveEmptyEntries)
                                 .ToList();

            allowedAppBundleIds = string.Join(";", bundleIds) + ";"; // make sure to append the last semicolon
        }

        return new AppClientData(allowedOrigins, allowedAppBundleIds, origins, bundleIds);
    }

    public record AppClientData(string AllowedOrigins, string AllowedAppBundleIds, List<string> Origins, List<string> BundleIds);

    private static CacheKey CacheKeyByClientId(string clientId) => new CacheKey($"AppClientManager:ClientId:{clientId}", cacheTime: 5);
    
    private static CacheKey CacheKeyByHostNameOrBundleId(string hostOrBundleId) => new CacheKey($"AppClientManager:HOstOrBundleId:{hostOrBundleId}", cacheTime: 5);
}