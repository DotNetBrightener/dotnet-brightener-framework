
using Microsoft.AspNetCore.Mvc;
using CRUDWebApiWithGeneratorDemo.Services.Data;
using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Controllers;

/// <summary>
///     Provide public APIs for <see cref="ProductCategory" /> entity.
/// </summary>
/// 
/// Uncomment the next line to enable authorization for this controller
/// [Authorize]
[ApiController]
[Route("api/[controller]")]
public partial class ProductCategoryController
{
    private readonly ILogger _logger;

    public ProductCategoryController(
            IProductCategoryDataService dataService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ProductCategoryController> logger)
        : this(dataService, httpContextAccessor)
    {
        _logger = logger;
    }

    // Implement or override APIs here
}