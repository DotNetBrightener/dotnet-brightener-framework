using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.DataTransferObjectUtility;
using DotNetBrightener.WebApi.GenericCRUD.Extensions;
using DotNetBrightener.WebApi.GenericCRUD.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace DotNetBrightener.WebApi.GenericCRUD.Controllers;

public abstract class BareReadOnlyController<TEntityType> : Controller where TEntityType : class
{
    /// <summary>
    ///     Specifies the name of the entity Id's column
    /// </summary>
    protected virtual string EntityIdColumnName => nameof(BaseEntity.Id);

    /// <summary>
    ///     The default query to fetch data
    /// </summary>
    protected virtual Expression<Func<TEntityType, bool>> DefaultQuery { get; set; }

    /// <summary>
    ///     The columns that will always be returned in case no columns is requested from the client
    /// </summary>
    protected string[] AlwaysReturnColumns;

    /// <summary>
    ///     Defines the default column of the data to return in case no columns is requested from the client
    /// </summary>
    /// <remarks>
    ///     This list will be concat with the <seealso cref="AlwaysReturnColumns"/>.
    ///     If this list is not specified, all available properties of the entity will be returned
    /// </remarks>
    protected virtual string[] DefaultColumnsToReturn { get; } = Array.Empty<string>();

    protected readonly IBaseDataService<TEntityType> DataService;
    protected readonly IHttpContextAccessor          HttpContextAccessor;

    private readonly string[] _alwaysIgnoreColumns = IgnoreColumnsTypeMappings.RetrieveIgnoreColumns<TEntityType>();

    protected BareReadOnlyController(IBaseDataService<TEntityType> dataService,
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
            AlwaysReturnColumns = AlwaysReturnColumns.Concat(new[]
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
        if (!await CanRetrieveList())
            throw new UnauthorizedAccessException();

        var adminQuery = BaseQueryModel.FromQuery(Request.Query);

        if (adminQuery?.DeletedRecordsOnly == true &&
            !await CanRetrieveDeletedItems())
        {
            return StatusCode((int)HttpStatusCode.Forbidden,
                              new
                              {
                                  ErrorMessage = $"Requesting for deleted entries is not allowed."
                              });
        }

        var entitiesQuery = adminQuery?.DeletedRecordsOnly == true
                                ? DataService.FetchDeletedRecords(DefaultQuery)
                                : DataService.FetchActive(DefaultQuery);

        return await GetListResult(entitiesQuery);
    }

    [HttpGet("{id:long}")]
    public virtual async Task<IActionResult> GetItem(long id)
    {
        if (!await CanRetrieveItem(id))
            throw new UnauthorizedAccessException();

        Expression<Func<TEntityType, bool>> expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        var adminQuery = BaseQueryModel.FromQuery(Request.Query);

        if (adminQuery?.DeletedRecordsOnly == true && !await CanRetrieveDeletedItems())
        {
            adminQuery.DeletedRecordsOnly = false;
        }

        var entityItemQuery = adminQuery?.DeletedRecordsOnly == true
                                  ? DataService.FetchDeletedRecords(DefaultQuery)
                                  : DataService.FetchActive(DefaultQuery);

        entityItemQuery = entityItemQuery.Where(expression);

        var finalQuery = await PerformColumnsSelectorQuery(entityItemQuery);

        var item = (await finalQuery.ToDynamicArrayAsync()).FirstOrDefault();

        if (item == null)
        {
            return StatusCode((int)HttpStatusCode.NotFound,
                              new
                              {
                                  ErrorMessage =
                                      $"The requested  {typeof(TEntityType).Name} resource with provided identifier cannot be found"
                              });
        }

        return Ok(item);
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
    ///     Considers if the current user can perform the <see cref="GetItem"/> action on deleted records
    /// </summary>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<bool> CanRetrieveDeletedItems()
    {
        return HttpContext.User.IsInRole("Administrator");
    }

    protected virtual Task<IActionResult> GetListResult(IQueryable<TEntityType> entitiesQuery)
        => GetListResult<TEntityType>(entitiesQuery);

    protected virtual async Task<IActionResult> GetListResult<TIn>(IQueryable<TIn> entitiesQuery) where TIn : class
    {
        entitiesQuery = await ApplyDeepFilters(entitiesQuery);

        var totalRecords = DynamicQueryableExtensions.Count(entitiesQuery);

        var orderedQuery = AddOrderingAndPaginationQuery(entitiesQuery,
                                                         out var pageSize,
                                                         out var pageIndex);

        var finalQuery = await PerformColumnsSelectorQuery(orderedQuery);

        return PagedListResult<TIn>(finalQuery, totalRecords, pageIndex, pageSize);
    }

    protected virtual IActionResult PagedListResult<TIn>(IQueryable finalQuery,
                                                         int        totalRecords,
                                                         int        pageIndex,
                                                         int        pageSize)
        where TIn : class
    {
        Response.Headers.Add("Result-Totals", totalRecords.ToString());
        Response.Headers.Add("Result-PageIndex", pageIndex.ToString());
        Response.Headers.Add("Result-PageSize", pageSize.ToString());

        var result = finalQuery.ToDynamicArray();
        Response.Headers.Add("Result-Count", result.Length.ToString());

        // add expose header to support CORS
        Response.Headers.Add("Access-Control-Expose-Headers",
                             "Result-Totals,Result-PageIndex,Result-PageSize,Result-Count," +
                             "Result-Totals,Result-PageIndex,Result-PageSize,Result-Count".ToLower());

        return Ok(result);
    }

    protected virtual Task<IQueryable<TEntityType>> ApplyDeepFilters(IQueryable<TEntityType> entitiesQuery) =>
        ApplyDeepFilters<TEntityType>(entitiesQuery);

    protected virtual Task<IQueryable<TIn>> ApplyDeepFilters<TIn>(IQueryable<TIn> entitiesQuery)
        where TIn : class
    {
        var deepPropertiesSearchFilters = Request.Query.ToDictionary(_ => _.Key,
                                                                     _ => _.Value.ToString());

        return entitiesQuery.ApplyDeepFilters(deepPropertiesSearchFilters);
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

        var filterDictionary = Request.Query.ToDictionary(_ => _.Key,
                                                          _ => _.Value.ToString());

        return entitiesQuery.AddOrderingAndPaginationQuery(filterDictionary,
                                                           EntityIdColumnName,
                                                           out pageSize,
                                                           out pageIndex);
    }

    /// <summary>
    ///     Add addition query into initial one to retrieve only provided columns from query string
    /// </summary>
    /// <param name="entitiesQuery">The initial query</param>
    /// <returns>The new query with additional operation, if any</returns>
    protected virtual Task<IQueryable> PerformColumnsSelectorQuery<TIn>(IQueryable<TIn> entitiesQuery) where TIn : class
    {
        var paginationQuery = BaseQueryModel.FromQuery(Request.Query);

        string[] columnsToReturn = paginationQuery.FilteredColumns
                                                  .Except(_alwaysIgnoreColumns)
                                                  .ToArray();

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
                                                                        string[]        columnsToReturn)
        where TIn : class
    {
        if (columnsToReturn == null ||
            columnsToReturn.Length == 0)
            return entitiesQuery;

        columnsToReturn = columnsToReturn.Except(_alwaysIgnoreColumns)
                                         .ToArray();

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
            MemberExpression mae                                    => mae.Member.Name,
            UnaryExpression { Operand: MemberExpression subSelect } => subSelect.Member.Name,
            _                                                       => ""
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
            UnaryExpression { Operand: MemberExpression mainSelect } =>
                mainSelect.Member.Name,
            _ => ""
        };

        if (string.IsNullOrEmpty(initProp))
            return "";

        return subSelector.Body switch
        {
            MemberExpression subSelectorBody => initProp + "." + subSelectorBody.Member.Name,
            UnaryExpression { Operand: MemberExpression subSelect } => initProp + "." +
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
            UnaryExpression { Operand: MemberExpression mainSelect } =>
                mainSelect.Member.Name,
            _ => ""
        };

        if (string.IsNullOrEmpty(initProp))
            return "";

        return subSelector.Body switch
        {
            MemberExpression subSelectorBody => initProp + "." + subSelectorBody.Member.Name,
            UnaryExpression { Operand: MemberExpression subSelect } => initProp + "." +
                                                                       subSelect.Member.Name,
            _ => ""
        };
    }
}