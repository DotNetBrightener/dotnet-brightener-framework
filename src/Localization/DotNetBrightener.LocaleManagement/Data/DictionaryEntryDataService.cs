
using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.Logging;

using LocaleManagement.Entities;

namespace LocaleManagement.Data;

/// <summary>
///     Provides the data access methods for <see cref="DictionaryEntry" /> entity.
/// </summary>
public partial interface IDictionaryEntryDataService 
{
    // Provide your custom methods here
}

public partial class DictionaryEntryDataService
{
    private readonly ILogger _logger;

    public DictionaryEntryDataService(
            IRepository repository, 
            ILogger<DictionaryEntryDataService> logger)
        : this(repository)
    {
        _logger = logger;
    }

    // Implement your custom methods here
}