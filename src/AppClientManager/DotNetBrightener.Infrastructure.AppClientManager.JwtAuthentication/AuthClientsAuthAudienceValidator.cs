using DotNetBrightener.Infrastructure.AppClientManager.Models;
using DotNetBrightener.Infrastructure.AppClientManager.Services;
using DotNetBrightener.Infrastructure.JwtAuthentication;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Infrastructure.AppClientManager.JwtAuthentication;

public class AuthClientsAuthAudienceValidator(IServiceScopeFactory serviceScopeFactory) : IAuthAudienceValidator
{
    public string[] GetValidAudiences()
    {
        using var scope           = serviceScopeFactory.CreateScope();
        var       serviceProvider = scope.ServiceProvider;

        var appClientDataService = serviceProvider.GetService<IAppClientManager>();

        var appClientOriginsAndBundles = appClientDataService!.GetAllAppClients()
                                                              .Where(appClient =>
                                                                         appClient is
                                                                         {
                                                                             IsDeleted   : false,
                                                                             ClientStatus: AppClientStatus.Active
                                                                         })
                                                              .Select(a => string.Join(";",
                                                               [
                                                                   a.AllowedOrigins,
                                                                   a.AllowedAppBundleIds
                                                               ]));

        var validAudiences = appClientOriginsAndBundles.SelectMany(originOrBundleId => originOrBundleId.Split([
                                                                                                                  ";",
                                                                                                                  ","
                                                                                                              ],
                                                                                                              StringSplitOptions
                                                                                                                 .RemoveEmptyEntries))
                                                       .Distinct()
                                                       .ToArray();

        return validAudiences;
    }
}