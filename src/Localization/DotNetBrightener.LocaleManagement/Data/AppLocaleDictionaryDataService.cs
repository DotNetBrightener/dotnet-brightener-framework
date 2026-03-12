/*
 * This is a user-customizable partial class file for AppLocaleDictionaryDataService.
 *
 * You can extend and customize the auto-generated functionality by:
 * - Overriding virtual/partial methods
 * - Adding custom properties and methods
 * - Implementing additional interfaces
 * - Adding custom attributes and configurations
 *
 * The core generated logic is in the corresponding *.g.cs file which is
 * embedded in the compilation and protected from modification.
 *
 * Entity: AppLocaleDictionary
 * Generated: 2025-07-24 15:18:37
 */

using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.Logging;

using LocaleManagement.Entities;

namespace LocaleManagement.Data;

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