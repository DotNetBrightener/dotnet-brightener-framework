/*
 * This is a user-customizable partial class file for ProductDataService.
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
 * Entity: Product
 * Generated: 2025-07-24 14:08:14
 */

using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.Logging;

using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Services.Data;

public partial class ProductDataService
{
    private readonly ILogger _logger;

    public ProductDataService(
            IRepository repository, 
            ILogger<ProductDataService> logger)
        : this(repository)
    {
        _logger = logger;
    }

    // Implement your custom methods here
}