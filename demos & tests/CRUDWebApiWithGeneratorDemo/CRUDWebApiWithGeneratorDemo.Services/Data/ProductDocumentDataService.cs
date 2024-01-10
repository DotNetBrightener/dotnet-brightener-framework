
using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.Logging;

using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Services.Data;

/// <summary>
///     Provides the data access methods for <see cref="ProductDocument" /> entity.
/// </summary>
public partial interface IProductDocumentDataService 
{
    // Provide your custom methods here
}

public partial class ProductDocumentDataService
{
    private readonly ILogger _logger;

    public ProductDocumentDataService(
            IRepository repository, 
            ILogger<ProductDocumentDataService> logger)
        : this(repository)
    {
        _logger = logger;
    }

    // Implement your custom methods here
}