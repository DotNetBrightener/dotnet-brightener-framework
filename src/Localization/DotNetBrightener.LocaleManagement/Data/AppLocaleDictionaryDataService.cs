
using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.Logging;

using LocaleManagement.Entities;

namespace LocaleManagement.Data;

/// <summary>
///     Provides the data access methods for <see cref="AppLocaleDictionary" /> entity.
/// </summary>
public partial interface IAppLocaleDictionaryDataService 
{
    // Provide your custom methods here
}

public partial class AppLocaleDictionaryDataService
{
    private readonly ILogger _logger;

    public AppLocaleDictionaryDataService(
            IRepository repository, 
            ILogger<AppLocaleDictionaryDataService> logger)
        : this(repository)
    {
        _logger = logger;
    }

    // Implement your custom methods here
}