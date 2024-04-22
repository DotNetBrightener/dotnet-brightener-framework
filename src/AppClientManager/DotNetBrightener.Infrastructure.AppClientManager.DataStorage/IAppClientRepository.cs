using DotNetBrightener.DataAccess;
using DotNetBrightener.DataAccess.EF.Repositories;
using DotNetBrightener.Infrastructure.AppClientManager.Models;
using DotNetBrightener.Infrastructure.AppClientManager.Services;
using DotNetBrightener.Plugins.EventPubSub;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.Infrastructure.AppClientManager.DataStorage;

public class AppClientRepository : Repository, IAppClientRepository
{
    public AppClientRepository(AppClientDbContext           dbContext,
                               ICurrentLoggedInUserResolver currentLoggedInUserResolver,
                               IEventPublisher              eventPublisher,
                               ILoggerFactory               loggerFactory)
        : base(dbContext, currentLoggedInUserResolver, eventPublisher, loggerFactory)
    {
    }
}

public class AppClientManager : IAppClientManager
{
    private readonly IAppClientRepository _appClientRepository;
    private readonly ILogger              _logger;

    public AppClientManager(IAppClientRepository      appClientRepository,
                            ILogger<AppClientManager> logger)
    {
        _logger              = logger;
        _appClientRepository = appClientRepository;
    }

    public async Task CreateAppClient(AppClient appClient)
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

    public async Task UpdateAppClient(AppClient appClient)
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

    public async Task<AppClient?> GetClientByHostNameOrByBundleId(string hostNameOrBundleId)
    {
        var condition = hostNameOrBundleId + ";";

        var appClient =
            _appClientRepository.Get<AppClient>(client => client.AllowedOrigins == hostNameOrBundleId ||
                                                          client.AllowedOrigins.Contains(condition));

        if (appClient != null)
        {
            return appClient;
        }

        return _appClientRepository.Get<AppClient>(client => client.AllowedAppBundleIds == hostNameOrBundleId ||
                                                             client.AllowedAppBundleIds.Contains(condition));
    }

    public async Task<AppClient?> GetClientByClientId(string clientId)
    {
        return _appClientRepository.Get<AppClient>(client => client.ClientId == clientId);
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
}