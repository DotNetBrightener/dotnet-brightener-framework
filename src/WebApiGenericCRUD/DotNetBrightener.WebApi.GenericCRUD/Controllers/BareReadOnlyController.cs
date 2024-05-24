using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.WebApi.GenericCRUD.Extensions;
using DotNetBrightener.GenericCRUD.Extensions;
using DotNetBrightener.GenericCRUD.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;

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
    protected virtual Expression<Func<TEntityType, bool>>? DefaultQuery { get; set; }

    /// <summary>
    ///     The columns that will always be returned in case no columns is requested from the client
    /// </summary>
    protected string[] AlwaysReturnColumns;

    protected readonly IBaseDataService<TEntityType> DataService;
    protected readonly IHttpContextAccessor          HttpContextAccessor;

    protected BareReadOnlyController(IBaseDataService<TEntityType> dataService,
                                     IHttpContextAccessor          httpContextAccessor)
    {
        DataService         = dataService;
        HttpContextAccessor = httpContextAccessor;

        AlwaysReturnColumns = [];

        if (typeof(BaseEntity<>).IsAssignableFrom(typeof(TEntityType)))
        {
            AlwaysReturnColumns =
            [
                nameof(BaseEntity.Id)
            ];
        }

        if (typeof(BaseEntityWithAuditInfo<>).IsAssignableFrom(typeof(TEntityType)))
        {
            AlwaysReturnColumns =
            [
                .. AlwaysReturnColumns,
                .. new[]
                {
                    nameof(BaseEntityWithAuditInfo.CreatedDate),
                    nameof(BaseEntityWithAuditInfo.CreatedBy),
                    nameof(BaseEntityWithAuditInfo.ModifiedDate),
                    nameof(BaseEntityWithAuditInfo.ModifiedBy),
                },
            ];
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
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    [HttpGet("")]
    public virtual async Task<IActionResult> GetList()
    {
        if (!await CanRetrieveList())
            throw new UnauthorizedAccessException();

        var currentRequestQueryStrings = Request.Query.ToQueryModel<BaseQueryModel>();

        if (!await ValidatePickColumns<TEntityType>(currentRequestQueryStrings.FilteredColumns,
                                                    out var invalidColumns))
        {
            var errorMessage = GetInvalidColumnsErrorMessage(invalidColumns);

            return StatusCode((int)HttpStatusCode.Forbidden,
                              new
                              {
                                  ErrorMessage = errorMessage,
                                  Data         = invalidColumns
                              });
        }

        if (currentRequestQueryStrings.DeletedRecordsOnly &&
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

        var result = await GetListResult(entitiesQuery,
                                         EntityIdColumnName,
                                         currentRequestQueryStrings.QueryDictionary,
                                         currentRequestQueryStrings.FilteredColumns);

        return result;
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

        var currentRequestQueryStrings = Request.Query.ToQueryModel<BaseQueryModel>();

        if (!await ValidatePickColumns<TEntityType>(currentRequestQueryStrings.FilteredColumns,
                                                    out var invalidColumns))
        {
            var errorMessage = GetInvalidColumnsErrorMessage(invalidColumns);

            return StatusCode((int)HttpStatusCode.Forbidden,
                              new
                              {
                                  ErrorMessage = errorMessage,
                                  Data         = invalidColumns
                              });
        }

        if (currentRequestQueryStrings.DeletedRecordsOnly &&
            !await CanRetrieveDeletedItems())
        {
            currentRequestQueryStrings.DeletedRecordsOnly = false;
        }

        var expression = EntityIdColumnName.EqualsTo<TEntityType>(id);

        var entityItemQuery = currentRequestQueryStrings.DeletedRecordsOnly
                                  ? DataService.FetchDeletedRecords(DefaultQuery)
                                  : DataService.FetchActive(DefaultQuery);

        entityItemQuery = entityItemQuery.Where(expression);

        var finalQuery = entityItemQuery.PerformColumnsSelectorQuery(currentRequestQueryStrings.FilteredColumns);

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
    ///     Retrieves the history records of <typeparamref name="TEntityType" /> record
    ///     for the given <paramref name="id"/>
    /// </summary>
    /// <param name="id">The identifier of the <typeparamref name="TEntityType"></typeparamref> record</param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    [HttpGet("{id:long}/_history")]
    public virtual async Task<IActionResult> GetItemHistory(long id)
    {
        if (!typeof(TEntityType).HasAttribute<HistoryEnabledAttribute>())
        {
            throw new NotSupportedException($"Entity type {typeof(TEntityType).Name} does not support versioning");
        }

        if (!await CanRetrieveItemHistory(id))
            throw new UnauthorizedAccessException();

        var currentRequestQueryStrings = Request.Query.ToQueryModel<BaseQueryModel>();

        if (!await ValidatePickColumns<TEntityType>(currentRequestQueryStrings.FilteredColumns,
                                                    out var invalidColumns))
        {
            var errorMessage = GetInvalidColumnsErrorMessage(invalidColumns);

            return StatusCode((int)HttpStatusCode.Forbidden,
                              new
                              {
                                  ErrorMessage = errorMessage,
                                  Data         = invalidColumns
                              });
        }

        var expression = EntityIdColumnName.EqualsTo<TEntityType>(id);

        var entityItemQuery = DataService.FetchHistory(expression);

        var result = await GetListResult(entityItemQuery,
                                         EntityIdColumnName,
                                         currentRequestQueryStrings.QueryDictionary,
                                         currentRequestQueryStrings.FilteredColumns);

        return result;
    }

    /// <summary>
    ///     Retrieves the error message for provided invalid columns
    /// </summary>
    /// <param name="invalidColumns">The invalid columns</param>
    /// <returns></returns>
    protected virtual string GetInvalidColumnsErrorMessage(List<string> invalidColumns)
    {
        var errorMessage =
            $"Some of the requested columns are not valid. The invalid columns are: [{string.Join(", ", invalidColumns)}].";

        return errorMessage;
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
    protected virtual Task<bool> ValidatePickColumns<TIn>(List<string>     queriedColumns,
                                                          out List<string> invalidColumns)
    {
        var alwaysIgnoreColumns = typeof(TIn).GetIgnoredProperties();

        invalidColumns = queriedColumns
                        .Where(c => alwaysIgnoreColumns.Any(aic =>
                                                                aic.Equals(c,
                                                                           StringComparison
                                                                              .OrdinalIgnoreCase)))
                        .ToList();

        var availableColumns = typeof(TIn).GetDefaultColumns()
                                          .OrderByDescending(_ => _.Length)
                                          .ToList();

        var correctedColumns = new List<string>();

        foreach (var queriedColumn in queriedColumns)
        {
            if (string.IsNullOrEmpty(queriedColumn))
            {
                continue;
            }

            Func<string, bool> equalPredicate = property =>
                property.Equals(queriedColumn,
                                StringComparison.OrdinalIgnoreCase) ;

            Func<string, bool> startsWithPredicate = property =>
                queriedColumn.StartsWith(property,
                                         StringComparison.OrdinalIgnoreCase);

            var columnAvailable = availableColumns.FirstOrDefault(equalPredicate) ??
                                  availableColumns.FirstOrDefault(startsWithPredicate);

            if (columnAvailable != null)
            {
                correctedColumns.Add(columnAvailable);
            }
            else
            {
                if (!invalidColumns.Contains(queriedColumn))
                    invalidColumns.Add(queriedColumn);
            }
        }

        queriedColumns.Clear();
        queriedColumns.AddRange(correctedColumns);

        return Task.FromResult(invalidColumns.Count == 0);
    }

    protected virtual async Task<IActionResult> GetListResult<TIn>(IQueryable<TIn>            entitiesQuery,
                                                                   string                     defaultSortColumnName,
                                                                   Dictionary<string, string> filterDictionary,
                                                                   List<string>               columnsToPick,
                                                                   Func<IQueryable<TIn>, IEnumerable<TIn>>
                                                                       postProcessing = null)
        where TIn : class
    {
        return await this.GeneratePagedListResult(entitiesQuery,
                                                  defaultSortColumnName,
                                                  filterDictionary,
                                                  columnsToPick,
                                                  postProcessing);
    }

    /// <summary>
    ///     Returns the property access name from the given selector
    /// </summary>
    /// <param name="selector">
    ///     The selector describes how to pick the property / column from the <typeparamref name="TEntityType"/>
    /// </param>
    protected static string PickProperty(Expression<Func<TEntityType, object>> selector)
        => selector.PickProperty();

    /// <summary>
    ///     Returns the property access name from the given selector and sub-sequence selector
    /// </summary>
    /// <param name="selector">
    ///     The selector describes how to pick the property / column from the <typeparamref name="TEntityType"/>
    /// </param>
    protected static string PickProperty<TNext>(Expression<Func<TEntityType, TNext>> selector,
                                                Expression<Func<TNext, object>>      subSelector)
        => selector.PickProperty(subSelector);

    /// <summary>
    ///     Returns the property access name from the given selector and sub-sequence selector
    /// </summary>
    /// <param name="selector">
    ///     The selector describes how to pick the property / column from the <typeparamref name="TEntityType"/>
    /// </param>
    protected static string PickProperty<TNext>(Expression<Func<TEntityType, IEnumerable<TNext>>> selector,
                                                Expression<Func<TNext, object>>                   subSelector)
        => selector.PickProperty(subSelector);

}