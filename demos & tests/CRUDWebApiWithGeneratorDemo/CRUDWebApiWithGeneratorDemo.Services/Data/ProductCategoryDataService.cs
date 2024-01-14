
using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.Logging;

using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Services.Data;

/// <summary>
///     Provides the data access methods for <see cref="ProductCategory" /> entity.
/// </summary>
public partial interface IProductCategoryDataService 
{
    // Provide your custom methods here
}

public partial class ProductCategoryDataService
{
    private readonly ILogger _logger;

    public ProductCategoryDataService(
            IRepository repository, 
            ILogger<ProductCategoryDataService> logger)
        : this(repository)
    {
        _logger = logger;
    }

    // Implement your custom methods here
}