/*
 * This is a user-customizable partial class file for ProductCategoryController.
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
 * Entity: ProductCategory
 * Generated: 2025-07-24 14:10:48
 */

using DotNetBrightener.WebApi.GenericCRUD.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRUDWebApiWithGeneratorDemo.Services.Data;
using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Controllers;

/// <summary>
///     Provide public APIs for <see cref="ProductCategory" /> entity.
/// </summary>
/// 
// Uncomment the next line to enable authorization for this controller
// [Authorize]
[ApiController]
[Route("api/[controller]")]
public partial class ProductCategoryController: BaseCRUDController<ProductCategory>
{
    private readonly ILogger _logger;

    public ProductCategoryController(
        IProductCategoryDataService dataService,
        ILogger<ProductCategoryController> logger)
        : this(dataService)
    {
        _logger = logger;
    }

    public override partial Task<IActionResult> GetList()
    {
        // override the base method to add your custom logic of loading collection of ProductCategory here

        return base.GetList();
    }

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

    protected override Task<(bool, ProductCategory, IActionResult)> CanDeleteItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can delete the ProductCategory item

        return base.CanDeleteItem(id);
    }

    protected override Task<(bool, ProductCategory, IActionResult)> CanRestoreDeletedItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can restore the ProductCategory item

        return base.CanRestoreDeletedItem(id);
    }
}