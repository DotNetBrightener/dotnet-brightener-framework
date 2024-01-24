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
using Microsoft.AspNetCore.Mvc.Filters;
using DotNetBrightener.DataAccess.Attributes;

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
    protected virtual string[] DefaultColumnsToReturn => 
        EntityMetadataExtractor.GetDefaultColumns<TEntityType>();

    /// <summary>
    ///     Specifies the list of columns that will always be ignored in the response
    /// </summary>
    protected virtual string[] AlwaysIgnoreColumns => IgnoreColumnsTypeMappings.RetrieveIgnoreColumns<TEntityType>();

    protected readonly IBaseDataService<TEntityType> DataService;
    protected readonly IHttpContextAccessor          HttpContextAccessor;

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

    /// <summary>
    ///     Retrieves the metadata of the <typeparamref name="TEntityType" />
    /// </summary>
    /// <response code="200">
    ///     The metadata information of the <typeparamref name="TEntityType"/> record.
    /// </response>
    /// <response code="401">
    ///     Unauthorized request to retrieve metadata information of <typeparamref name="TEntityType"/> API.
    /// </response> 
    /// <response code="500">
    ///     Unknown internal server error.
    /// </response>
    [ProducesResponseType(typeof(EntityMetadata<>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    [HttpGet("_metadata")]
    public virtual async Task<IActionResult> GetMetadata()
    {
        if (!await CanRetrieveEndpointMetadata())
            throw new UnauthorizedAccessException();

        var metadata = EntityMetadataExtractor.ExtractMetadata<TEntityType>();

        return Ok(metadata);
    }

    /// <summary>
    ///     Retrieves the collection of records of type <typeparamref name="TEntityType" />.
    /// </summary>
    /// <typeparam name="TEntityType">The type of the entity associated with this controller</typeparam>
    /// <response code="200">
    ///     The collection of records of type <typeparamref name="TEntityType" />.
    /// </response>
    /// <response code="401">
    ///     Unauthorized request to retrieve filtered collection of <typeparamref name="TEntityType" />.records.
    /// </response> 
    /// <response code="500">
    ///     Unknown internal server error.
    /// </response>
    [ProducesResponseType(typeof(IQueryable<>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    [HttpGet("")]
    public virtual async Task<IActionResult> GetList()
    {
        if (!await CanRetrieveList())
            throw new UnauthorizedAccessException();

        var currentRequestQueryStrings = BaseQueryModel.FromQuery(Request.Query);

        if (!await VerifyPickColumns<TEntityType>(currentRequestQueryStrings.FilteredColumns,
                                                  out string[] invalidColumns))
        {
            return StatusCode((int)HttpStatusCode.Forbidden,
                              new
                              {
                                  ErrorMessage =
                                      $"Some of the requested columns are not valid. The invalid columns are: [{string.Join(", ", invalidColumns)}].",
                                  Data = invalidColumns
                              });
        }

        if (currentRequestQueryStrings?.DeletedRecordsOnly == true &&
            !await CanRetrieveDeletedItems())
        {
            return StatusCode((int)HttpStatusCode.Forbidden,
                              new
                              {
                                  ErrorMessage = $"Requesting for deleted entries is not allowed."
                              });
        }

        var entitiesQuery = currentRequestQueryStrings?.DeletedRecordsOnly == true
                                ? DataService.FetchDeletedRecords(DefaultQuery)
                                : DataService.FetchActive(DefaultQuery);

        return await GetListResult(entitiesQuery);
    }

    /// <summary>
    ///     Retrieves the <typeparamref name="TEntityType"></typeparamref> record with the given <paramref name="id"/>
    /// </summary>
    /// <param name="id">The identifier of the <typeparamref name="TEntityType"></typeparamref> record</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    [HttpGet("{id:long}")]
    public virtual async Task<IActionResult> GetItem(long id)
    {
        if (!await CanRetrieveItem(id))
            throw new UnauthorizedAccessException();

        var currentRequestQueryStrings = BaseQueryModel.FromQuery(Request.Query);

        if (!await VerifyPickColumns<TEntityType>(currentRequestQueryStrings.FilteredColumns, 
                                                  out string[] invalidColumns))
        {
            return StatusCode((int)HttpStatusCode.Forbidden,
                              new
                              {
                                  ErrorMessage =
                                      $"Some of the requested columns are not valid. The invalid columns are: [{string.Join(", ", invalidColumns)}].",
                                  Data = invalidColumns
                              });
        }

        if (currentRequestQueryStrings?.DeletedRecordsOnly == true &&
            !await CanRetrieveDeletedItems())
        {
            currentRequestQueryStrings.DeletedRecordsOnly = false;
        }

        Expression<Func<TEntityType, bool>> expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        var entityItemQuery = currentRequestQueryStrings?.DeletedRecordsOnly == true
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
    ///     Retrieves the history of <typeparamref name="TEntityType"></typeparamref> record with the given <paramref name="id"/>
    /// </summary>
    /// <param name="id">The identifier of the <typeparamref name="TEntityType"></typeparamref> record</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    [HttpGet("{id:long}/_history")]
    public virtual async Task<IActionResult> GetItemHistory(long id)
    {
        if (!typeof(TEntityType).HasAttribute<HistoryEnabledAttribute>())
        {
            throw new NotSupportedException($"Entity type {typeof(TEntityType).Name} does not support versioning");
        }

        if (!await CanRetrieveItemHistory(id))
            throw new UnauthorizedAccessException();

        var currentRequestQueryStrings = BaseQueryModel.FromQuery(Request.Query);

        if (!await VerifyPickColumns<TEntityType>(currentRequestQueryStrings.FilteredColumns, 
                                                  out string[] invalidColumns))
        {
            return StatusCode((int)HttpStatusCode.Forbidden,
                              new
                              {
                                  ErrorMessage =
                                      $"Some of the requested columns are not valid. The invalid columns are: [{string.Join(", ", invalidColumns)}].",
                                  Data = invalidColumns
                              });
        }

        Expression<Func<TEntityType, bool>> expression =
            ExpressionExtensions.BuildPredicate<TEntityType>(id, OperatorComparer.Equals, EntityIdColumnName);

        var entityItemQuery = DataService.FetchHistory(expression);

        return await GetListResult(entityItemQuery);
    }

    /// <summary>
    ///     Considers if the current user can perform the <see cref="GetMetadata"/> action
    /// </summary>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<bool> CanRetrieveEndpointMetadata()
    {
        return true;
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
    ///     Considers if the current user can perform the <see cref="GetItemHistory"/> action
    /// </summary>
    /// <returns>
    ///     <c>true</c> if user is authorized to perform the action; otherwise, <c>false</c>
    /// </returns>
    protected virtual async Task<bool> CanRetrieveItemHistory(long id)
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


    /// <summary>
    ///     Verifies if the columns requested to be returned are valid
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <param name="queriedColumns">
    ///     The columns defined in the query string
    /// </param>
    /// <param name="invalidColumns">
    ///     The array of invalid columns returned after the verification
    /// </param>
    /// <returns>
    ///     <c>true</c> if the requested columns are valid; otherwise, <c>false</c>
    /// </returns>
    protected virtual Task<bool> VerifyPickColumns<TIn>(string[] queriedColumns, out string[] invalidColumns)
    {
        invalidColumns = queriedColumns
                        .Where(_ => AlwaysIgnoreColumns.Any(ignoreColumn =>
                                                                ignoreColumn.Equals(_,
                                                                                    StringComparison
                                                                                       .OrdinalIgnoreCase)))
                        .ToArray();
        
        invalidColumns = queriedColumns
                        .Where(requestingColumn => !string.IsNullOrEmpty(requestingColumn) &&
                                                   !DefaultColumnsToReturn
                                                      .Any(property =>
                                                               property.Equals(requestingColumn,
                                                                               StringComparison
                                                                                  .OrdinalIgnoreCase) ||
                                                               requestingColumn.StartsWith(property,
                                                                                           StringComparison
                                                                                              .OrdinalIgnoreCase)))
                        .Concat(invalidColumns)
                        .Distinct()
                        .ToArray();

        return Task.FromResult(invalidColumns.Length == 0);
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
        Response.Headers.Append("Result-Totals", totalRecords.ToString());
        Response.Headers.Append("Result-PageIndex", pageIndex.ToString());
        Response.Headers.Append("Result-PageSize", pageSize.ToString());

        var result = finalQuery.ToDynamicArray();
        Response.Headers.Append("Result-Count", result.Length.ToString());

        // add expose header to support CORS
        Response.Headers.Append("Access-Control-Expose-Headers",
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
                                                  .Except(AlwaysIgnoreColumns)
                                                  .ToArray();

        if (columnsToReturn.Length == 0)
        {
            columnsToReturn = DefaultColumnsToReturn.Concat(AlwaysReturnColumns)
                                                    .Where(_ => !string.IsNullOrEmpty(_))
                                                    .Except(AlwaysIgnoreColumns)
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

        columnsToReturn = columnsToReturn.Except(AlwaysIgnoreColumns)
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

    // filters 

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!DefaultColumnsToReturn.Any())
        {
            context.Result = StatusCode((int)HttpStatusCode.InternalServerError,
                                        new
                                        {
                                            ErrorMessage =
                                                "Internal Server Error. The DefaultColumnsToReturn variable must have values"
                                        });
        }

        base.OnActionExecuting(context);
    }
}