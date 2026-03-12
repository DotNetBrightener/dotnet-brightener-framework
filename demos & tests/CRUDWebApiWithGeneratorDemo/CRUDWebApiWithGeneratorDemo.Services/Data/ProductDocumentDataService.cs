
using DotNetBrightener.DataAccess.Services;
using Microsoft.Extensions.Logging;

using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Services.Data;

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