using DotNetBrightener.Infrastructure.AppClientManager.Models;

namespace DotNetBrightener.Infrastructure.AppClientManager.Services;

public class InMemoryAppClientManager : IAppClientManager
{
    private static readonly List<AppClient> AppClients = new();

    public Task CreateAppClient(AppClient appClient)
    {
        if (AppClients.Exists(_ => _.ClientId == appClient.ClientId))
            throw new InvalidOperationException($"AppClient with ClientId '{appClient.ClientId}' already exists.");

        AppClients.Add(appClient);

        return Task.CompletedTask;
    }

    public Task UpdateAppClient(AppClient appClient)
    {
        var existingAppClient = AppClients.Find(_ => _.ClientId == appClient.ClientId);

        if (existingAppClient == null)
            throw new InvalidOperationException($"AppClient with ClientId '{appClient.ClientId}' does not exist.");

        AppClients.Remove(existingAppClient);
        AppClients.Add(appClient);

        return Task.CompletedTask;
    }

    public List<AppClient> GetAllAppClients()
    {
        return AppClients;
    }

    public async Task<AppClient> GetClientByHostNameOrByBundleId(string hostNameOrBundleId)
    {
        var appClient = AppClients.Find(a => a.AllowedOrigins == hostNameOrBundleId + ";" ||
                                              a.AllowedOrigins?.Contains(hostNameOrBundleId + ";") == true ||
                                              a.AllowedAppBundleIds == hostNameOrBundleId + ";" ||
                                              a.AllowedAppBundleIds?.Contains(hostNameOrBundleId + ";") == true);

        return appClient;
    }

    public Task<AppClient> GetClientByClientId(string clientId)
    {
        var appClient = AppClients.Find(_ => _.ClientId == clientId);

        return Task.FromResult(appClient);
    }
}