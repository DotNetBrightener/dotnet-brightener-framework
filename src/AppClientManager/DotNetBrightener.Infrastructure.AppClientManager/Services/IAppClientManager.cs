using DotNetBrightener.Infrastructure.AppClientManager.Models;

namespace DotNetBrightener.Infrastructure.AppClientManager.Services;

public interface IAppClientManager
{
    Task CreateAppClient(AppClient appClient);

    Task UpdateAppClient(AppClient appClient);

    List<AppClient> GetAllAppClients();

    Task<AppClient> GetClientByHostNameOrByBundleId(string hostNameOrBundleId);

    Task<AppClient> GetClientByClientId(string clientId);
}