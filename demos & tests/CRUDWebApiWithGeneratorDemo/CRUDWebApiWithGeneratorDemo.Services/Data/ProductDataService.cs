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