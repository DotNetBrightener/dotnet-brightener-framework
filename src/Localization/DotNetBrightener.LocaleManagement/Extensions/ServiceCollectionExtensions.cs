using DotNetBrightener.LocaleManagement.Data;
using DotNetBrightener.LocaleManagement.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.LocaleManagement.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLocaleManagementModule(this IServiceCollection services)
    {

        services.AddScoped<IAppLocaleDictionaryDataService, AppLocaleDictionaryDataService>();
        services.AddScoped<IDictionaryEntryDataService, DictionaryEntryDataService>();
        services.AddScoped<ILocaleManagementService, LocaleManagementService>();

        return services;
    }
}
