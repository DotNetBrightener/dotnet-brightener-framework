using DotNetBrightener.Infrastructure.AppClientManager.Models;

namespace DotNetBrightener.Infrastructure.AppClientManager.Services;

public class InMemoryAppClientManager : IAppClientManager
{
    public Task CreateAppClient(AppClient appClient)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAppClient(AppClient appClient)
    {
        throw new NotImplementedException();
    }

    public List<AppClient> GetAllAppClients()
    {
        throw new NotImplementedException();
    }

    public Task<AppClient?> GetClientByHostNameOrByBundleId(string hostNameOrBundleId)
    {
        throw new NotImplementedException();
    }

    public Task<AppClient?> GetClientByClientId(string clientId)
    {
        throw new NotImplementedException();
    }
}