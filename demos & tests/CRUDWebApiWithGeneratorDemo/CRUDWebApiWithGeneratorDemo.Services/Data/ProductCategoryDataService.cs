
using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.Logging;

using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Services.Data;

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