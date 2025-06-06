using DotNetBrightener.WebApi.GenericCRUD.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRUDWebApiWithGeneratorDemo.Services.Data;
using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Controllers;

/// <summary>
///     Provide public APIs for <see cref="ProductDocument" /> entity.
/// </summary>
/// 
// Uncomment the next line to enable authorization for this controller
// [Authorize]
[ApiController]
[Route("api/[controller]")]
public partial class ProductDocumentController: BaseCRUDController<ProductDocument>
{
    private readonly ILogger _logger;

    public ProductDocumentController(
        IProductDocumentDataService dataService,
        ILogger<ProductDocumentController> logger)
        : this(dataService)
    {
        _logger = logger;
    }

    public override partial Task<IActionResult> GetList()
    {
        // override the base method to add your custom logic of loading collection of ProductDocument here

        return base.GetList();
    }

    protected override Task<bool> CanRetrieveList()
    {
        // override the base method to add your custom logic of checking
        // if the current user can retrieve the list of ProductDocument records

        return base.CanRetrieveList();
    }

    protected override Task<bool> CanRetrieveItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can retrieve the ProductDocument item by its id

        return base.CanRetrieveItem(id);
    }

    protected override Task<bool> CanCreateItem(ProductDocument entityItem)
    {
        // override the base method to add your custom logic of checking
        // if the current user can create a new ProductDocument item

        return base.CanCreateItem(entityItem);
    }

    protected override Task<(bool, ProductDocument, IActionResult)> CanUpdateItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can update the ProductDocument item

        return base.CanUpdateItem(id);
    }

    protected override Task<(bool, ProductDocument, IActionResult)> CanDeleteItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can delete the ProductDocument item

        return base.CanDeleteItem(id);
    }

    protected override Task<(bool, ProductDocument, IActionResult)> CanRestoreDeletedItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can restore the ProductDocument item

        return base.CanRestoreDeletedItem(id);
    }
}