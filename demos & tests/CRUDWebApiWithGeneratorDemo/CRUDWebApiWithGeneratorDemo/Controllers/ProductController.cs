using DotNetBrightener.WebApi.GenericCRUD.Controllers;

using Microsoft.AspNetCore.Mvc;
using CRUDWebApiWithGeneratorDemo.Services.Data;
using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Controllers;

/// <summary>
///     Provide public APIs for <see cref="Product" /> entity.
/// </summary>
/// 
// Uncomment the next line to enable authorization for this controller
// [Authorize]
[ApiController]
[Route("api/[controller]")]
public partial class ProductController: BaseCRUDController<Product>
{
    private readonly ILogger _logger;

    public ProductController(
        IProductDataService dataService,
        ILogger<ProductController> logger)
        : this(dataService)
    {
        _logger = logger;
    }

    public override partial Task<IActionResult> GetList()
    {
        // override the base method to add your custom logic of loading collection of Product here

        return base.GetList();
    }

    protected override Task<bool> CanRetrieveList()
    {
        // override the base method to add your custom logic of checking
        // if the current user can retrieve the list of Product records

        return base.CanRetrieveList();
    }

    protected override Task<bool> CanRetrieveItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can retrieve the Product item by its id

        return base.CanRetrieveItem(id);
    }

    protected override Task<bool> CanCreateItem(Product entityItem)
    {
        // override the base method to add your custom logic of checking
        // if the current user can create a new Product item

        return base.CanCreateItem(entityItem);
    }

    protected override Task<(bool, Product, IActionResult)> CanUpdateItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can update the Product item

        return base.CanUpdateItem(id);
    }

    protected override Task<(bool, Product, IActionResult)> CanDeleteItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can delete the Product item

        return base.CanDeleteItem(id);
    }

    protected override Task<(bool, Product, IActionResult)> CanRestoreDeletedItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can restore the Product item

        return base.CanRestoreDeletedItem(id);
    }
}