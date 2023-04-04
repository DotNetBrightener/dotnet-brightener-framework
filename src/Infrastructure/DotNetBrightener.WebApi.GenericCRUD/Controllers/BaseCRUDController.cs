using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.WebApi.GenericCRUD.ActionFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using DotNetBrightener.DataTransferObjectUtility;
using System.Globalization;

namespace DotNetBrightener.WebApi.GenericCRUD.Controllers;

// ReSharper disable once InconsistentNaming
public abstract class BaseCRUDController<TEntityType> : Controller where TEntityType : class
{
    /// <summary>
    ///     Specifies the name of the entity Id's column
    /// </summary>
    protected virtual string EntityIdColumnName => nameof(BaseEntity.Id);

    protected readonly IBaseDataService<TEntityType> DataService;
    protected readonly IHttpContextAccessor          HttpContextAccessor;

    /// <summary>
    ///     The default query to fetch data
    /// </summary>
    protected virtual Expression<Func<TEntityType, bool>> DefaultQuery { get; set; }

    /// <summary>
    ///     The columns that will be always returned in case no columns is requested from the client
    /// </summary>
    protected string [ ] AlwaysReturnColumns;

    /// <summary>
    ///     Defines the default column of the data to return in case no columns is requested from the client
    /// </summary>
    /// <remarks>
    ///     This list will be concat with the <seealso cref="AlwaysReturnColumns"/>.
    ///     If this list is not specified, all available properties of the entity will be returned
    /// </remarks>
    protected virtual string [ ] DefaultColumnsToReturn { get; } = Array.Empty<string>();

    protected BaseCRUDController(IBaseDataService<TEntityType> dataService,
                                 IHttpContextAccessor          httpContextAccessor)
    {
        DataService         = dataService;
        HttpContextAccessor = httpContextAccessor;
        AlwaysReturnColumns = Array.Empty<string>();

        if (typeof(BaseEntity).IsAssignableFrom(typeof(TEntityType)))
        {
            AlwaysReturnColumns = new[]
            {
                nameof(BaseEntity.Id)
            };
        }

        if (typeof(BaseEntityWithAuditInfo).IsAssignableFrom(typeof(TEntityType)))
        {
            AlwaysReturnColumns = AlwaysReturnColumns.Concat(new [ ]
                                                      {
                                                          nameof(BaseEntityWithAuditInfo.CreatedDate),
                                                          nameof(BaseEntityWithAuditInfo.CreatedBy),
                                                          nameof(BaseEntityWithAuditInfo.ModifiedDate),
                                                          nameof(BaseEntityWithAuditInfo.ModifiedBy),
                                                      })
                                                     .ToArray();
        }
    }

    [HttpGet("")]
    public virtual async Task<IActionResult> GetList()
    {
        if (!(await CanRetrieveList()))
            throw new UnauthorizedAccessException();

        var entitiesQuery = DataService.FetchActive(DefaultQuery);
        entitiesQuery = await ApplyDeepFilters(entitiesQuery);

        var totalRecords = DynamicQueryableExtensions.Count(entitiesQuery);

        var orderedQuery = AddOrderingAndPaginationQuery(entitiesQuery, out var pageSize, out var pageIndex);
        var finalQuery   = await PerformColumnsSelectorQuery(orderedQuery);

        Response.Headers.Add("Result-Totals", totalRecords.ToString());
        Response.Headers.Add("Result-PageIndex", pageIndex.ToString());
        Response.Headers.Add("Result-PageSize", pageSize.ToString());

        var result = finalQuery.ToDynamicArray();
        Response.Headers.Add("Result-Count", result.Count().ToString());

        // add expose header to support CORS
        Response.Headers.Add("Access-Control-Expose-Headers",
                             "Result-Totals,Result-PageIndex,Result-PageSize,Result-Count," +
                             "Result-Totals,Result-PageIndex,Result-PageSize,Result-Count".ToLower());

        return Ok(result);
    }

    [HttpGet("{id:long}")]
    public virtual async Task<IActionResult> GetItem(long id)
    {
        if (!(await CanRetrieveItem(id)))
            throw new UnauthorizedAccessException();

        Expression<Func<TEntityType, bool>> expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        var entityItemQuery = DynamicQueryableExtensions.Where(DataService.FetchActive(DefaultQuery), expression);

        var finalQuery = await PerformColumnsSelectorQuery(entityItemQuery);

        var item = finalQuery.FirstOrDefault() ?? null;

        if (item is null)
        {
            return StatusCode((int) HttpStatusCode.NotFound,
                              new 
                              {
                                  ErrorMessage =
                                      $"The requested  {typeof(TEntityType).Name} resource with provided identifier cannot be found"
                              });
        }

        return Ok(item);
    }

    [HttpPost("")]
    [RequestBodyToString]
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
    [RequestBodyToString]
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

        var entityToUpdate = RequestBodyToStringAttribute.ObtainBodyAsJObject(HttpContextAccessor);

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
                                      auditableEntity.CreatedDate,
                                      auditableEntity.CreatedBy,
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
    ///     Considers if the current user can perform the <see cref="GetList"/> action
    /// </summary>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<bool> CanRetrieveList()
    {
        return true;
    }

    /// <summary>
    ///     Considers if the current user can perform the <see cref="GetItem"/> action
    /// </summary>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<bool> CanRetrieveItem(long id)
    {
        return true;
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

    protected virtual Task<IQueryable<TEntityType>> ApplyDeepFilters(IQueryable<TEntityType> entitiesQuery)
    {
        return ApplyDeepFilters<TEntityType>(entitiesQuery);
    }

    protected virtual async Task<IQueryable<TIn>> ApplyDeepFilters<TIn>(IQueryable<TIn> entitiesQuery)
        where TIn : class
    {
        var deepPropertiesSearchFilters = Request.Query.ToDictionary(_ => _.Key,
                                                                     _ => _.Value.ToString());

        if (deepPropertiesSearchFilters.Keys.Count == 0)
            return entitiesQuery;

        var predicateStatement = PredicateBuilder.True<TIn>();
        var textInfo           = new CultureInfo("en-US", false).TextInfo;

        foreach (var filter in deepPropertiesSearchFilters)
        {
            var property = typeof(TIn).GetProperty(filter.Key);

            if (property == null)
            {
                var filterKeyTitleCase = textInfo.ToTitleCase(filter.Key);
                property = typeof(TIn).GetProperty(filterKeyTitleCase);
            }

            if (property == null)
                continue;

            if (property.PropertyType == typeof(string))
            {
                var filterValue = filter.Value.Replace("*", "");

                if (filter.Value.StartsWith("*") &&
                    filter.Value.EndsWith("*"))
                {
                    var predicateQuery =
                        ExpressionExtensions.BuildPredicate<TIn>(filterValue,
                                                                 OperatorComparer.Contains,
                                                                 property.Name);
                    predicateStatement = predicateStatement.And(predicateQuery);
                }

                else if (filter.Value.EndsWith("*"))
                {
                    var predicateQuery =
                        ExpressionExtensions.BuildPredicate<TIn>(filterValue,
                                                                 OperatorComparer.StartsWith,
                                                                 property.Name);
                    predicateStatement = predicateStatement.And(predicateQuery);
                }

                else if (filter.Value.StartsWith("*"))
                {
                    var predicateQuery =
                        ExpressionExtensions.BuildPredicate<TIn>(filterValue,
                                                                 OperatorComparer.EndsWith,
                                                                 property.Name);
                    predicateStatement = predicateStatement.And(predicateQuery);
                }
            }
        }

        return entitiesQuery.Where(predicateStatement);
    }

    /// <summary>
    ///     Add addition query into initial one to order the result, and optionally pagination
    /// </summary>
    /// <param name="entitiesQuery">The initial query</param>
    /// <returns>The new query with additional operation, if any</returns>
    protected virtual IQueryable<TIn> AddOrderingAndPaginationQuery<TIn>(IQueryable<TIn> entitiesQuery,
                                                                         out int         pageSize,
                                                                         out int         pageIndex)
        where TIn : class
    {
        var paginationQuery = BaseQueryModel.FromQuery(Request.Query);

        pageSize = paginationQuery.PageSize;

        if (pageSize == 0)
        {
            pageSize = 20;
        }

        pageIndex = paginationQuery.PageIndex;

        var orderedEntitiesQuery = entitiesQuery.OrderBy(ExpressionExtensions
                                                            .BuildMemberAccessExpression<TIn>(EntityIdColumnName));

        if (paginationQuery.OrderedColumns.Length > 0)
        {
            var sortIndex = 0;

            foreach (var orderByColumn in paginationQuery.OrderedColumns)
            {
                var actualColumnName = orderByColumn;

                if (orderByColumn.StartsWith("-"))
                {
                    actualColumnName = orderByColumn.Substring(1);
                    var orderByColumnExpr =
                        ExpressionExtensions.BuildMemberAccessExpression<TIn>(actualColumnName);

                    orderedEntitiesQuery = sortIndex == 0
                                               ? entitiesQuery.OrderByDescending(orderByColumnExpr)
                                               : orderedEntitiesQuery.ThenByDescending(orderByColumnExpr);
                }
                else
                {
                    var orderByColumnExpr =
                        ExpressionExtensions.BuildMemberAccessExpression<TIn>(actualColumnName);
                    orderedEntitiesQuery = sortIndex == 0
                                               ? entitiesQuery.OrderBy(orderByColumnExpr)
                                               : orderedEntitiesQuery.ThenBy(orderByColumnExpr);
                }

                sortIndex++;
            }
        }

        var itemsToSkip = pageIndex * pageSize;
        var itemsToTake = pageSize;

        return orderedEntitiesQuery.Skip(itemsToSkip)
                                   .Take(itemsToTake);
    }

    /// <summary>
    ///     Add addition query into initial one to retrieve only provided columns from query string
    /// </summary>
    /// <param name="entitiesQuery">The initial query</param>
    /// <returns>The new query with additional operation, if any</returns>
    protected virtual Task<IQueryable> PerformColumnsSelectorQuery<TIn>(IQueryable<TIn> entitiesQuery) where TIn: class
    {
        var paginationQuery = BaseQueryModel.FromQuery(Request.Query);

        string [ ] columnsToReturn = paginationQuery.FilteredColumns;

        if (columnsToReturn.Length == 0 &&
            DefaultColumnsToReturn.Any())
        {
            columnsToReturn = DefaultColumnsToReturn.Concat(AlwaysReturnColumns)
                                                    .Where(_ => !string.IsNullOrEmpty(_))
                                                    .Distinct()
                                                    .ToArray();
        }

        return ApplyPickColumnsQuery(entitiesQuery, columnsToReturn);
    }

    /// <summary>
    ///     Add addition query into initial one to retrieve the entity with only given <seealso cref="columnsToReturn"/>
    /// </summary>
    /// <param name="entitiesQuery">
    ///     The initial query
    /// </param>
    /// <param name="columnsToReturn">
    ///     List of properties of the entity to query
    /// </param>
    /// <returns>The new query with additional operation, if any</returns>
    protected virtual async Task<IQueryable> ApplyPickColumnsQuery<TIn>(IQueryable<TIn> entitiesQuery,
                                                                        string [ ]      columnsToReturn)
        where TIn : class
    {
        if (columnsToReturn == null ||
            columnsToReturn.Length == 0)
            return entitiesQuery;

        if (typeof(BaseEntity).IsAssignableFrom(typeof(TEntityType)) &&
            columnsToReturn.All(_ => !_.Equals(nameof(BaseEntity.Id), StringComparison.OrdinalIgnoreCase)))
        {
            // always return Id even if the client does not ask
            columnsToReturn = new List<string>
                {
                    nameof(BaseEntity.Id)
                }.Concat(columnsToReturn)
                 .ToArray();
        }

        var filteredResult =
            entitiesQuery.Select(DataTransferObjectUtils.BuildDtoSelectorExpressionFromEntity<TIn>(columnsToReturn));

        return filteredResult;
    }

    /// <summary>
    ///     Returns the property access name from the given selector
    /// </summary>
    /// <param name="selector">
    ///     The selector describes how to pick the property / column from the <typeparamref name="TEntityType"/>
    /// </param>
    protected static string PickProperty(Expression<Func<TEntityType, object>> selector) =>
        PickProperty<TEntityType>(selector);


    /// <summary>
    ///     Returns the property access name from the given selector
    /// </summary>
    /// <param name="selector">
    ///     The selector describes how to pick the property / column from the <typeparamref name="TEntityType"/>
    /// </param>
    protected static string PickProperty<TIn>(Expression<Func<TIn, object>> selector)
    {
        return selector.Body switch
        {
            MemberExpression mae                                  => mae.Member.Name,
            UnaryExpression {Operand: MemberExpression subSelect} => subSelect.Member.Name,
            _                                                     => ""
        };
    }

    /// <summary>
    ///     Returns the property access name from the given selector and sub-sequence selector
    /// </summary>
    /// <param name="selector">
    ///     The selector describes how to pick the property / column from the <typeparamref name="TEntityType"/>
    /// </param>
    protected static string PickProperty<TNext>(Expression<Func<TEntityType, TNext>> selector,
                                                Expression<Func<TNext, object>>      subSelector)
        => PickProperty<TEntityType, TNext>(selector, subSelector);

    /// <summary>
    ///     Returns the property access name from the given selector and sub-sequence selector
    /// </summary>
    /// <param name="selector">
    ///     The selector describes how to pick the property / column from the <typeparamref name="TEntityType"/>
    /// </param>
    protected static string PickProperty<TNext>(Expression<Func<TEntityType, IEnumerable<TNext>>> selector,
                                                Expression<Func<TNext, object>>                   subSelector)
        => PickProperty<TEntityType, TNext>(selector, subSelector);

    /// <summary>
    ///     Returns the property access name from the given selector and sub-sequence selector
    /// </summary>
    /// <param name="selector">
    ///     The selector describes how to pick the property / column from the <typeparamref name="TEntityType"/>
    /// </param>
    protected static string PickProperty<TIn, TNext>(Expression<Func<TIn, IEnumerable<TNext>>> selector,
                                                     Expression<Func<TNext, object>>           subSelector)
    {
        string initProp = selector.Body switch
        {
            MemberExpression mae => mae.Member.Name,
            UnaryExpression {Operand: MemberExpression mainSelect} =>
                mainSelect.Member.Name,
            _ => ""
        };

        if (string.IsNullOrEmpty(initProp))
            return "";

        return subSelector.Body switch
        {
            MemberExpression subSelectorBody => initProp + "." + subSelectorBody.Member.Name,
            UnaryExpression {Operand: MemberExpression subSelect} => initProp + "." +
                                                                     subSelect.Member.Name,
            _ => ""
        };
    }

    /// <summary>
    ///     Returns the property access name from the given selector and sub-sequence selector
    /// </summary>
    /// <param name="selector">
    ///     The selector describes how to pick the property / column from the <typeparamref name="TEntityType"/>
    /// </param>
    protected static string PickProperty<TIn, TNext>(Expression<Func<TIn, TNext>>    selector,
                                                     Expression<Func<TNext, object>> subSelector)
    {
        string initProp = selector.Body switch
        {
            MemberExpression mae => mae.Member.Name,
            UnaryExpression {Operand: MemberExpression mainSelect} =>
                mainSelect.Member.Name,
            _ => ""
        };

        if (string.IsNullOrEmpty(initProp))
            return "";

        return subSelector.Body switch
        {
            MemberExpression subSelectorBody => initProp + "." + subSelectorBody.Member.Name,
            UnaryExpression {Operand: MemberExpression subSelect} => initProp + "." +
                                                                     subSelect.Member.Name,
            _ => ""
        };
    }
}