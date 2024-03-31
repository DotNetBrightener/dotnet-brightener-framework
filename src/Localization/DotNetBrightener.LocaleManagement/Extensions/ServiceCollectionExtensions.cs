using LocaleManagement.Data;
using LocaleManagement.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LocaleManagement.Extensions;
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
