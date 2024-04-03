
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


    public override partial Task<IActionResult> GetList()
    {
        // override the base method to add your custom logic of loading collection of ProductCategory here

        return base.GetList();
    }

    #region Override Authorization Methods

    protected override Task<bool> CanRetrieveList()
    {
        // override the base method to add your custom logic of checking
        // if the current user can retrieve the list of ProductCategory records

        return base.CanRetrieveList();
    }

    protected override Task<bool> CanRetrieveItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can retrieve the ProductCategory item by its id

        return base.CanRetrieveItem(id);
    }

    protected override Task<bool> CanCreateItem(ProductCategory entityItem)
    {
        // override the base method to add your custom logic of checking
        // if the current user can create a new ProductCategory item

        return base.CanCreateItem(entityItem);
    }

    protected override Task<(bool, ProductCategory, IActionResult)> CanUpdateItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can update the ProductCategory item

        return base.CanUpdateItem(id);
    }

    protected override Task<bool> CanDeleteItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can delete the ProductCategory item

        return base.CanDeleteItem(id);
    }

    protected override Task<bool> CanRestoreDeletedItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can restore the ProductCategory item

        return base.CanRestoreDeletedItem(id);
    }

    #endregion
}