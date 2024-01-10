
using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.Logging;

using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Services.Data;

/// <summary>
///     Provides the data access methods for <see cref="Product" /> entity.
/// </summary>
public partial interface IProductDataService 
{
    // Provide your custom methods here
}

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