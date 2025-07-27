
using DotNetBrightener.WebApi.GenericCRUD.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRUDWebApiWithGeneratorDemo.Services.Data;
using CRUDWebApiWithGeneratorDemo.Core.Entities;

namespace CRUDWebApiWithGeneratorDemo.Controllers;

/// <summary>
///     Provide public APIs for <see cref="GroupEntity" /> entity.
/// </summary>
/// 
// Uncomment the next line to enable authorization for this controller
// [Authorize]
[ApiController]
[Route("api/[controller]")]
public partial class GroupEntityController: BaseCRUDController<GroupEntity>
{
    private readonly ILogger _logger;

    public GroupEntityController(
        IGroupEntityDataService dataService,
        ILogger<GroupEntityController> logger)
        : this(dataService)
    {
        _logger = logger;
    }

    public override partial Task<IActionResult> GetList()
    {
        // override the base method to add your custom logic of loading collection of GroupEntity here

        return base.GetList();
    }

    protected override Task<bool> CanRetrieveList()
    {
        // override the base method to add your custom logic of checking
        // if the current user can retrieve the list of GroupEntity records

        return base.CanRetrieveList();
    }

    protected override Task<bool> CanRetrieveItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can retrieve the GroupEntity item by its id

        return base.CanRetrieveItem(id);
    }

    protected override Task<bool> CanCreateItem(GroupEntity entityItem)
    {
        // override the base method to add your custom logic of checking
        // if the current user can create a new GroupEntity item

        return base.CanCreateItem(entityItem);
    }

    protected override Task<(bool, GroupEntity, IActionResult)> CanUpdateItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can update the GroupEntity item

        return base.CanUpdateItem(id);
    }

    protected override Task<(bool, GroupEntity, IActionResult)> CanDeleteItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can delete the GroupEntity item

        return base.CanDeleteItem(id);
    }

    protected override Task<(bool, GroupEntity, IActionResult)> CanRestoreDeletedItem(long id)
    {
        // override the base method to add your custom logic of checking
        // if the current user can restore the GroupEntity item

        return base.CanRestoreDeletedItem(id);
    }
}