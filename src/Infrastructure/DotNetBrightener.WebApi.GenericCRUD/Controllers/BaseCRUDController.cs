using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.DataTransferObjectUtility;
using DotNetBrightener.WebApi.GenericCRUD.ActionFilters;
using DotNetBrightener.WebApi.GenericCRUD.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace DotNetBrightener.WebApi.GenericCRUD.Controllers;

// ReSharper disable once InconsistentNaming
public abstract class BaseCRUDController<TEntityType> : BareReadOnlyController<TEntityType> where TEntityType : class
{
    protected BaseCRUDController(IBaseDataService<TEntityType> dataService,
                                 IHttpContextAccessor          httpContextAccessor)
        : base(dataService, httpContextAccessor)
    {
        
    }

    /// <summary>
    ///    Creates a new <typeparamref name="TEntityType" /> record in the database with the provided data
    /// </summary>
    /// <typeparam name="TEntityType">The type of entity record</typeparam>
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

        if (model is BaseEntity baseEntity)
        {
            return StatusCode((int)HttpStatusCode.Created, GetCreatedResult(baseEntity));
        }

        return StatusCode((int)HttpStatusCode.Created);
    }

    /// <summary>
    ///     Updates the <typeparamref name="TEntityType"/> record in the database with the provided data
    /// </summary>
    /// <typeparam name="TEntityType">The type of entity record</typeparam>
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
    [RequestBodyReader]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public virtual async Task<IActionResult> UpdateItem(long id, [FromBody] TEntityType entityItem)
    {
        var (canUpdate, entity, result) = await CanUpdateItem(id);

        if (result is not null)
            return result;

        if (!canUpdate || entity is null)
            throw new UnauthorizedAccessException();

        var entityToUpdate = RequestBodyReader.ObtainBodyAsJObject(HttpContextAccessor);

        var auditTrail = await UpdateEntity(entity, entityToUpdate);

        DataService.Update(entity);

        await PostUpdateEntity(entity);

        if (entity is BaseEntity baseEntity)
        {
            if (entity is BaseEntityWithAuditInfo auditableEntity)
            {
                return StatusCode((int)HttpStatusCode.OK,
                                  new
                                  {
                                      EntityId = baseEntity.Id,
                                      auditableEntity.ModifiedDate,
                                      auditableEntity.ModifiedBy
                                  });
            }

            return StatusCode((int)HttpStatusCode.OK,
                              new
                              {
                                  EntityId = baseEntity.Id
                              });
        }

        return StatusCode((int)HttpStatusCode.OK);
    }

    /// <summary>
    ///     Deletes the <typeparamref name="TEntityType"/> record from the database
    /// </summary>
    /// <typeparam name="TEntityType">The type of entity record</typeparam>
    /// <param name="id">
    ///     The identifier of the <typeparamref name="TEntityType"/> record to delete
    /// </param> 
    /// <response code="200">The record is deleted successfully.</response>
    /// <response code="401">Unauthorized request to delete <typeparamref name="TEntityType"/> record.</response> 
    /// <response code="500">Unknown internal server error.</response>
    [HttpDelete("{id:long}")]
    public virtual async Task<IActionResult> DeleteItem(long id)
    {
        if (!await CanDeleteItem(id))
            throw new UnauthorizedAccessException();

        var expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        DataService.DeleteOne(expression);

        return StatusCode((int) HttpStatusCode.OK);
    }

    /// <summary>
    ///     Restores the deleted <typeparamref name="TEntityType"/> record from the database
    /// </summary>
    /// <typeparam name="TEntityType">The type of entity record</typeparam>
    /// <param name="id">
    ///     The identifier of the deleted <typeparamref name="TEntityType"/> record
    /// </param> 
    /// <response code="200">The record is restored successfully.</response>
    /// <response code="401">Unauthorized request restore the <typeparamref name="TEntityType"/> record.</response> 
    /// <response code="500">Unknown internal server error.</response>
    [HttpPut("{id:long}/undelete")]
    public virtual async Task<IActionResult> RestoreDeletedItem(long id)
    {
        if (!await CanRestoreDeletedItem(id))
            throw new UnauthorizedAccessException();

        var expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        DataService.RestoreOne(expression);

        return StatusCode((int) HttpStatusCode.OK);
    }

    protected async Task<bool> AuthorizedCreateItem(TEntityType entityItem) => await CanCreateItem(entityItem);

    /// <summary>
    ///     Considers if the current user is authorized to do the <see cref="CreateItem"/> action
    /// </summary>
    /// <remarks>
    ///     If the <see cref="entityType"/> needs to be assigned to the logged in user,
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
        var expression = ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);
        var entity = DataService.Get(expression);


        return (true, entity, entity is null ? NotFound() : null);
    }

    /// <summary>
    ///     Considers if the current user can perform the <see cref="DeleteItem"/> action
    /// </summary>
    /// <param name="id">The identifier of the entry to check for deletion permission</param>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<bool> CanDeleteItem(long id)
    {
        return true;
    }

    /// <summary>
    ///     Considers if the current user can perform the <see cref="RestoreDeletedItem"/> action
    /// </summary>
    /// <param name="id">The identifier of the entry to check for deletion permission</param>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<bool> CanRestoreDeletedItem(long id)
    {
        return true;
    }

    /// <summary>
    ///     Performs the necessary updates to the <see cref="entityToPersist"/> from the <see cref="dataToUpdate"/>
    /// </summary>
    /// <remarks>
    ///     The logic to implement in the derived classes are specific for how to save the <typeparamref name="TEntityType"/>.
    ///     If the logic is complex, consider to call the Business Service layer to perform the necessary updates
    /// </remarks>
    /// <param name="entityToPersist">
    ///     The data to be persisted into the data storage
    /// </param>
    /// <param name="dataToUpdate">
    ///     The data came from the request contains the changes needed.
    /// </param>
    /// <returns></returns>
    protected virtual Task<AuditTrail<TEntityType>> UpdateEntity(TEntityType entityToPersist, object dataToUpdate)
    {
        var ignoreProperties = typeof(TEntityType).GetPropertiesWithNoClientSideUpdate();

        DataTransferObjectUtils.UpdateEntityFromDto(entityToPersist,
                                                    dataToUpdate,
                                                    out var auditTrail,
                                                    ignoreProperties);

        return Task.FromResult(auditTrail);
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

    protected virtual CreatedEntityResultModel GetCreatedResult(BaseEntity baseEntity)
    {
        if (baseEntity is BaseEntityWithAuditInfo auditableEntity)
        {
            return new CreatedEntityResultModel(auditableEntity);
        }

        return new CreatedEntityResultModel(baseEntity);
    }
}