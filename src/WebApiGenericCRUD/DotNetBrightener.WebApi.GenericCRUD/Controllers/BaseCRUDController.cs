using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Models.Guards;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.GenericCRUD.Models;
using DotNetBrightener.WebApi.GenericCRUD.ActionFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Net;

namespace DotNetBrightener.WebApi.GenericCRUD.Controllers;

// ReSharper disable once InconsistentNaming
public abstract class BaseCRUDController<TEntityType>(IBaseDataService<TEntityType> dataService)
    : BareReadOnlyController<TEntityType>(dataService)
    where TEntityType : class
{
    /// <summary>
    ///    Creates a new <typeparamref name="TEntityType" /> record in the database with the provided data
    /// </summary>
    /// <param name="model">
    ///     The data to be inserted into the database
    /// </param> 
    /// <response code="201">The new record is created successfully.</response>
    /// <response code="401">Unauthorized request to create a new <typeparamref name="TEntityType"/> record.</response> 
    /// <response code="500">Unknown internal server error.</response>
    [HttpPost("")]
    [RequestBodyReader]
    [ProducesResponseType<CreatedEntityResultModel>(201)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public virtual async Task<IActionResult> CreateItem([FromBody] TEntityType model)
    {
        if (!await CanCreateItem(model))
            throw new UnauthorizedAccessException();

        await PreCreateItem(model);

        await DataService.InsertAsync(model);

        await PostCreateItem(model);

        if (model is IBaseEntity baseEntity)
        {
            return StatusCode((int)HttpStatusCode.Created, GetCreatedResult(baseEntity));
        }

        return StatusCode((int)HttpStatusCode.Created);
    }

    /// <summary>
    ///     Updates the <typeparamref name="TEntityType"/> record in the database with the provided data
    /// </summary>
    /// <param name="id">
    ///     The identifier of the <typeparamref name="TEntityType"/> record to update
    /// </param>
    /// <param name="entityItem">
    ///     The data of updated <typeparamref name="TEntityType"/> record
    /// </param> 
    /// <response code="200">The record is updated successfully.</response>
    /// <response code="401">Unauthorized request to update the <typeparamref name="TEntityType"/> record.</response> 
    /// <response code="500">Unknown internal server error.</response>
    [HttpPut("{id:long}")]
    [HttpPatch("{id:long}")]
    [RequestBodyReader]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public virtual async Task<IActionResult> UpdateItem(long id, [FromBody] TEntityType entityItem)
    {
        var (canUpdate, entity, result) = await CanUpdateItem(id);

        if (result is not null)
            return result;

        if (!canUpdate ||
            entity is null)
            throw new UnauthorizedAccessException();

        var entityToUpdate = HttpContext.ObtainRequestBodyAsJObject();

        await DataService.UpdateAsync(entity, entityToUpdate);

        await PostUpdateEntity(entity);

        if (entity is IAuditableEntity auditableEntity)
        {
            return StatusCode((int)HttpStatusCode.OK,
                              new
                              {
                                  EntityId = id,
                                  auditableEntity.ModifiedDate,
                                  auditableEntity.ModifiedBy
                              });
        }

        return StatusCode((int)HttpStatusCode.OK,
                          new
                          {
                              EntityId = id
                          });

    }

    /// <summary>
    ///     Deletes the <typeparamref name="TEntityType"/> record from the database
    /// </summary>
    /// <param name="id">
    ///     The identifier of the <typeparamref name="TEntityType"/> record to delete
    /// </param> 
    /// <response code="200">The record is deleted successfully.</response>
    /// <response code="401">Unauthorized request to delete <typeparamref name="TEntityType"/> record.</response> 
    /// <response code="500">Unknown internal server error.</response>
    [HttpDelete("{id:long}")]
    public virtual async Task<IActionResult> DeleteItem(long id)
    {
        var (canDelete, entity, result) = await CanDeleteItem(id);

        if (result is not null)
            return result;

        if (!canDelete ||
            entity is null)
            throw new UnauthorizedAccessException();

        var expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        await DataService.DeleteOne(expression);

        return StatusCode((int)HttpStatusCode.OK);
    }

    /// <summary>
    ///     Restores the deleted <typeparamref name="TEntityType"/> record from the database
    /// </summary>
    /// <param name="id">
    ///     The identifier of the deleted <typeparamref name="TEntityType"/> record
    /// </param> 
    /// <response code="200">The record is restored successfully.</response>
    /// <response code="401">Unauthorized request restore the <typeparamref name="TEntityType"/> record.</response> 
    /// <response code="500">Unknown internal server error.</response>
    [HttpPut("{id:long}/undelete")]
    public virtual async Task<IActionResult> RestoreDeletedItem(long id)
    {
        var (canRestore, entity, result) = await CanRestoreDeletedItem(id);

        if (result is not null)
            return result;

        if (!canRestore ||
            entity is null)
            throw new UnauthorizedAccessException();

        var expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        await DataService.RestoreOne(expression);

        return StatusCode((int)HttpStatusCode.OK);
    }

    /// <summary>
    ///     Considers if the current user is authorized to do the <see cref="CreateItem"/> action
    /// </summary>
    /// <remarks>
    ///     If the <see cref="entityType"/> needs to be assigned to the logged-in user,
    ///     you need to do that assignment in the override method of this
    /// </remarks>
    /// <param name="entityItem">
    ///     The entity object
    /// </param>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<bool> CanCreateItem(TEntityType entityItem)
    {
        return true;
    }

    /// <summary>
    ///     Considers if the current user can perform the <see cref="UpdateItem"/> action
    /// </summary>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<(bool, TEntityType, IActionResult)> CanUpdateItem(long id)
    {
        Expression<Func<TEntityType, bool>> expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        var entity = DataService.Get(expression);

        return (entity is not null, entity, entity is null ? NotFound() : null);
    }

    /// <summary>
    ///     Considers if the current user can perform the <see cref="DeleteItem"/> action
    /// </summary>
    /// <param name="id">The identifier of the entry to check for deletion permission</param>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<(bool, TEntityType, IActionResult)> CanDeleteItem(long id)
    {
        Expression<Func<TEntityType, bool>> expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        var entity = DataService.Get(expression);

        return (entity is not null, entity, entity is null ? NotFound() : null);
    }

    /// <summary>
    ///     Considers if the current user can perform the <see cref="RestoreDeletedItem"/> action
    /// </summary>
    /// <param name="id">The identifier of the entry to check for deletion permission</param>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<(bool, TEntityType, IActionResult)> CanRestoreDeletedItem(long id)
    {
        Guards.AssertEntityRecoverable<TEntityType>();

        Expression<Func<TEntityType, bool>> expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        var entity = DataService.Get(expression);

        return (entity is not null, entity, entity is null ? NotFound() : null);
    }

    /// <summary>
    ///     Performs some action prior to item being inserted into the database.
    ///     Exception thrown during this method will interrupt the insertion, prevent it from continuing
    /// </summary>
    /// <param name="entity">The entity object that will be inserted into the database</param>
    /// <returns></returns>
    protected virtual Task PreCreateItem(TEntityType entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Performs some actions after the item has been inserted into the database.
    ///     Exception thrown in this method will not prevent the entity record from being inserted, unless the <seealso cref="CreateItem"/> method is overriden
    /// </summary>
    /// <param name="entity">The entity object that was inserted into the database</param>
    /// <returns></returns>
    protected virtual Task PostCreateItem(TEntityType entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Performs some actions after the item has been updated into the database.
    ///     Exception thrown in this method will not prevent the entity record from being updated,
    ///     unless the <seealso cref="UpdateItem"/> method is overriden
    /// </summary>
    /// <param name="entity">The entity object that was inserted into the database</param>
    /// <returns></returns>
    protected virtual Task PostUpdateEntity(TEntityType entity)
    {
        return Task.CompletedTask;
    }

    protected virtual CreatedEntityResultModel GetCreatedResult(IBaseEntity baseEntity)
    {
        CreatedEntityResultModel createdEntityResultModel;

        if (baseEntity is IAuditableEntity auditableEntity)
        {
            createdEntityResultModel = new CreatedEntityResultModel(auditableEntity);
        }
        else
        {
            createdEntityResultModel = new CreatedEntityResultModel();
        }

        if (baseEntity is BaseEntity ett)
        {
            createdEntityResultModel.EntityId = ett.Id;
        }

        return createdEntityResultModel;
    }
}