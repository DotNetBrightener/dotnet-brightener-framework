using DotNetBrightener.LocaleManagement.Data;
using DotNetBrightener.LocaleManagement.Services;
using DotNetBrightener.LocaleManagement.WebApi.Controllers;

namespace LocaleManagement.WebAPI.Controllers;

public class LocaleManagementController : BaseLocaleManagementServiceController
{
    public LocaleManagementController(ILocaleManagementService            localeManagementService,
                                      IAppLocaleDictionaryDataService     appLocaleDictionaryDataService,
                                      IDictionaryEntryDataService         dictionaryEntryDataService,
                                      ILogger<LocaleManagementController> logger)
        : base(localeManagementService, appLocaleDictionaryDataService, dictionaryEntryDataService, logger)
    {
    }

    public override Task<bool> CanRetrieveDictionary(string appId = null)
    {
        return Task.FromResult<bool>(true);
    }

    public override Task<bool> CanRetrieveLocales(string appId = null)
    {
        return Task.FromResult<bool>(true);
    }

    public override Task<bool> CanCreateDictionary()
    {
        return Task.FromResult<bool>(true);
    }

    public override Task<bool> CanDeleteDictionary()
    {
        return Task.FromResult<bool>(true);
    }

    public override Task<bool> CanUpdateDictionary()
    {
        return Task.FromResult<bool>(true);
    }

    public override Task<bool> CanDeleteDictionaryEntry()
    {
        return Task.FromResult<bool>(true);
    }
}