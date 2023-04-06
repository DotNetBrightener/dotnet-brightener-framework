using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.DataTransferObjectUtility;
using DotNetBrightener.WebApi.GenericCRUD.ActionFilters;
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

    [HttpPost("")]
    [RequestBodyReader]
    public virtual async Task<IActionResult> CreateItem([FromBody]
                                                        TEntityType model)
    {
        if (!(await AuthorizedCreateItem(model)))
            throw new UnauthorizedAccessException();

        await PreCreateItem(model);

        await DataService.InsertAsync(model);

        await PostCreateItem(model);

        if (model is BaseEntity baseEntity)
        {
            if (model is BaseEntityWithAuditInfo auditableEntity)
            {
                return StatusCode((int)HttpStatusCode.Created,
                                  new
                                  {
                                      EntityId = baseEntity.Id,
                                      auditableEntity.CreatedDate,
                                      auditableEntity.CreatedBy,
                                      auditableEntity.ModifiedDate,
                                      auditableEntity.ModifiedBy
                                  });
            }

            return StatusCode((int) HttpStatusCode.Created,
                              new
                              {
                                  EntityId = baseEntity.Id
                              });
        }

        return StatusCode((int) HttpStatusCode.Created);
    }

    [HttpPut("{id:long}")]
    [RequestBodyReader]
    public virtual async Task<IActionResult> UpdateItem(long id)
    {
        if (!(await CanUpdateItem(id)))
            throw new UnauthorizedAccessException();

        var expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        var entity = DataService.Get(expression);

        if (entity == null)
        {
            return StatusCode((int)HttpStatusCode.NotFound,
                              new
                              {
                                  ErrorMessage =
                                      $"The requested  {typeof(TEntityType).Name} resource with provided identifier cannot be found"
                              });
        }

        var entityToUpdate = RequestBodyReader.ObtainBodyAsJObject(HttpContextAccessor);

        await UpdateEntity(entity, entityToUpdate);

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

    [HttpDelete("{id:long}")]
    public virtual async Task<IActionResult> DeleteItem(long id)
    {
        if (!(await CanDeleteItem(id)))
            throw new UnauthorizedAccessException();

        var expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        DataService.DeleteOne(expression);

        return StatusCode((int) HttpStatusCode.OK);
    }

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
    protected virtual async Task<bool> AuthorizedCreateItem(TEntityType entityItem)
    {
        return true;
    }

    /// <summary>
    ///     Considers if the current user can perform the <see cref="UpdateItem"/> action
    /// </summary>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<bool> CanUpdateItem(long id)
    {
        return true;
    }

    /// <summary>
    ///     Considers if the current user can perform the <see cref="DeleteItem"/> action
    /// </summary>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<bool> CanDeleteItem(long id)
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
    protected virtual Task UpdateEntity(TEntityType entityToPersist, object dataToUpdate)
    {
        var ignoreProperties = typeof(TEntityType).GetPropertiesWithNoClientSideUpdate();

        DataTransferObjectUtils.UpdateEntityFromDto(entityToPersist, dataToUpdate, ignoreProperties);

        return Task.CompletedTask;
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

    protected virtual Task PostUpdateEntity(TEntityType entity)
    {
        return Task.CompletedTask;
    }
}