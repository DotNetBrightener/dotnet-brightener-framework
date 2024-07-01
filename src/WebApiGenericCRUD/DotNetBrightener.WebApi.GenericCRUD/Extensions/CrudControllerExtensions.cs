using System.Linq.Dynamic.Core;
using DotNetBrightener.GenericCRUD.Extensions;
using DotNetBrightener.GenericCRUD.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotNetBrightener.WebApi.GenericCRUD.Extensions;

public static class CrudControllerExtensions
{

    /// <summary>
    ///     Generates the paged list result from the given <paramref name="entitiesQuery"/>
    /// </summary>
    /// <typeparam name="TIn">Type of the entity in the given <see cref="entitiesQuery"/></typeparam>
    /// <param name="controller">The <see cref="Controller"/> instance</param>
    /// <param name="entitiesQuery">
    ///     The query that returns the collection of <typeparamref name="TIn"/> to be paged
    /// </param>
    /// <param name="defaultSortColumnName">
    ///     The name of the column to be used as default sort column if no sort column is provided
    /// </param>
    /// <param name="filterDictionary">
    ///     The dictionary that contains the filter information
    /// </param>
    /// <param name="columnsToPick">
    ///     The list of columns to be picked from the <typeparamref name="TIn"/> entity
    /// </param>
    /// <returns></returns>
    public static async Task<IActionResult> GeneratePagedListResult<TIn>(this Controller controller,
                                                                         IQueryable<TIn> entitiesQuery,
                                                                         string defaultSortColumnName,
                                                                         Dictionary<string, string> filterDictionary,
                                                                         List<string> columnsToPick)
        where TIn : class
    {

        var paginationQuery = filterDictionary.ToQueryModel<BaseQueryModel>();

        entitiesQuery = entitiesQuery.ApplyDeepFilters(filterDictionary);

        var totalRecords = DynamicQueryableExtensions.Count(entitiesQuery);

        var orderedQuery = entitiesQuery.AddOrderingAndPaginationQuery(filterDictionary,
                                                                       defaultSortColumnName);

        var finalQuery = orderedQuery.PerformColumnsSelectorQuery(columnsToPick);

        return controller.GetPagedListResult(finalQuery,
                                             totalRecords,
                                             paginationQuery.PageIndex,
                                             paginationQuery.PageSize);
    }

    /// <summary>
    ///     Generates the paged list result from the given <paramref name="entitiesQuery"/>
    /// </summary>
    /// <typeparam name="TIn">Type of the entity in the given <see cref="entitiesQuery"/></typeparam>
    /// <param name="controller">The <see cref="Controller"/> instance</param>
    /// <param name="entitiesQuery">
    ///     The query that returns the collection of <typeparamref name="TIn"/> to be paged
    /// </param>
    /// <param name="defaultSortColumnName">
    ///     The name of the column to be used as default sort column if no sort column is provided
    /// </param>
    /// <param name="filterDictionary">
    ///     The dictionary that contains the filter information
    /// </param>
    /// <param name="columnsToPick">
    ///     The list of columns to be picked from the <typeparamref name="TIn"/> entity
    /// </param>
    /// <returns></returns>
    public static async Task<IActionResult> GeneratePagedListResult<TIn>(this Controller controller,
                                                                         IQueryable<TIn> entitiesQuery,
                                                                         string defaultSortColumnName,
                                                                         Dictionary<string, string> filterDictionary,
                                                                         List<string> columnsToPick,
                                                                         Func<IQueryable<TIn>, IEnumerable<TIn>> postProcessing)
        where TIn : class
    {

        var paginationQuery = filterDictionary.ToQueryModel<BaseQueryModel>();

        entitiesQuery = entitiesQuery.ApplyDeepFilters(filterDictionary);

        var totalRecords = DynamicQueryableExtensions.Count(entitiesQuery);

        var orderedQuery = entitiesQuery.AddOrderingAndPaginationQuery(filterDictionary,
                                                                       defaultSortColumnName);

        if (postProcessing is not null)
        {
            orderedQuery = postProcessing(orderedQuery).AsQueryable();
        }

        var finalQuery = orderedQuery.PerformColumnsSelectorQuery(columnsToPick);

        return controller.GetPagedListResult(finalQuery,
                                             totalRecords,
                                             paginationQuery.PageIndex,
                                             paginationQuery.PageSize);
    }

    /// <summary>
    ///     Generates the paged list result from the given <paramref name="entitiesQuery"/>
    /// </summary>
    /// <typeparam name="TIn">Type of the entity in the given <see cref="entitiesQuery"/></typeparam>
    /// <param name="controller">The <see cref="Controller"/> instance</param>
    /// <param name="entitiesQuery">
    ///     The query that returns the collection of <typeparamref name="TIn"/> to be paged
    /// </param>
    /// <param name="defaultSortColumnName">
    ///     The name of the column to be used as default sort column if no sort column is provided
    /// </param>
    /// <param name="filterDictionary">
    ///     The dictionary that contains the filter information
    /// </param>
    /// <param name="columnsToPick">
    ///     The list of columns to be picked from the <typeparamref name="TIn"/> entity
    /// </param>
    /// <returns></returns>
    public static async Task<IActionResult> GeneratePagedListResult<TIn>(this Controller controller,
                                                                         IQueryable<TIn> entitiesQuery,
                                                                         string defaultSortColumnName,
                                                                         Dictionary<string, string> filterDictionary,
                                                                         List<string> columnsToPick,
                                                                         Func<IQueryable<TIn>, Task<IEnumerable<TIn>>>
                                                                             postProcessing)
        where TIn : class
    {

        var paginationQuery = filterDictionary.ToQueryModel<BaseQueryModel>();

        entitiesQuery = entitiesQuery.ApplyDeepFilters(filterDictionary);

        var totalRecords = DynamicQueryableExtensions.Count(entitiesQuery);

        var orderedQuery = entitiesQuery.AddOrderingAndPaginationQuery(filterDictionary,
                                                                       defaultSortColumnName);

        if (postProcessing is not null)
        {
            orderedQuery = (await postProcessing(orderedQuery)).AsQueryable();
        }

        var finalQuery = orderedQuery.PerformColumnsSelectorQuery(columnsToPick);

        return controller.GetPagedListResult(finalQuery,
                                             totalRecords,
                                             paginationQuery.PageIndex,
                                             paginationQuery.PageSize);
    }


    /// <summary>
    ///     Gets the result of the paged collection in <see cref="finalQuery"/> and response to the client
    /// </summary>
    /// <param name="controller">The <see cref="Controller"/></param>
    /// <param name="finalQuery">The final query that returns the paged collection</param>
    /// <param name="totalRecords">The number of all records for the collection</param>
    /// <param name="pageIndex">The current index of page</param>
    /// <param name="pageSize">The number of records requested for a page</param>
    public static IActionResult GetPagedListResult(this Controller controller,
                                                   IQueryable      finalQuery,
                                                   int             totalRecords,
                                                   int             pageIndex,
                                                   int             pageSize)
    {
        IHeaderDictionary responseHeaders = controller.Response.Headers;

        responseHeaders.AppendTotalCount(totalRecords);
        responseHeaders.AppendPageSize(pageSize);
        responseHeaders.AppendPageIndex(pageIndex);

        var result = finalQuery.ToDynamicArray();
        responseHeaders.AppendResultCount(result.Length);

        return controller.Ok(result);
    }
}