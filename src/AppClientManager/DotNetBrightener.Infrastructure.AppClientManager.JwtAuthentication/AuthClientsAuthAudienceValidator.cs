using DotNetBrightener.Infrastructure.AppClientManager.Models;
using DotNetBrightener.Infrastructure.AppClientManager.Services;
using DotNetBrightener.Infrastructure.JwtAuthentication;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Infrastructure.AppClientManager.JwtAuthentication;

public class AuthClientsAuthAudienceValidator : IAuthAudienceValidator
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public AuthClientsAuthAudienceValidator(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public string[] GetValidAudiences()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var appClientDataService = serviceProvider.GetService<IAppClientManager>();

        var authClientAppUrls = appClientDataService!.GetAllAppClients()
                                                     .Where(appClient =>
                                                                appClient is
                                                                {
                                                                    IsDeleted: false,
                                                                    ClientStatus: AppClientStatus.Active
                                                                })
                                                     .Select(a => new
                                                     {
                                                         a.AllowedOrigins,
                                                         a.AllowedAppBundleIds
                                                     })
                                                     .ToArray();

        var validAudiences = authClientAppUrls.SelectMany(_ => _.AllowedOrigins.Split([
                                                                                          ";", ","
                                                                                      ],
                                                                                      StringSplitOptions
                                                                                         .RemoveEmptyEntries))
                                              .Concat(authClientAppUrls.SelectMany(_ => _.AllowedAppBundleIds
                                                                                         .Split([
                                                                                                    ";", ","
                                                                                                ],
                                                                                                StringSplitOptions
                                                                                                   .RemoveEmptyEntries)))
                                              .Distinct()
                                              .ToArray();

        return validAudiences;
    }
}