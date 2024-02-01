using DotNetBrightener.DataAccess.Models;
using DotNetBrightener.DataTransferObjectUtility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace DotNetBrightener.WebApi.GenericCRUD.Extensions;

public static partial class QueryableDeepFilterExtensions
{
    /// <summary>
    ///     Add addition query into initial one to retrieve only provided columns from query string
    /// </summary>
    /// <param name="entitiesQuery">The initial query</param>
    /// <param name="columnsToPick"></param>
    /// <returns>The new query with additional operation, if any</returns>
    public static IQueryable PerformColumnsSelectorQuery<TIn>(this IQueryable<TIn> entitiesQuery,
                                                              List<string> columnsToPick = null) where TIn : class
    {
        if (columnsToPick is null ||
            columnsToPick.Count == 0)
        {
            return entitiesQuery;
        }

        var alwaysIgnoreColumns = typeof(TIn).GetIgnoredProperties();

        var columnsToReturn = columnsToPick.Except(alwaysIgnoreColumns)
                                           .ToArray();

        if (typeof(BaseEntity).IsAssignableFrom(typeof(TIn)) &&
            columnsToReturn.All(columnName =>
                                    !columnName.Equals(nameof(BaseEntity.Id), StringComparison.OrdinalIgnoreCase)))
        {
            columnsToReturn = new[]
                {
                    nameof(BaseEntity.Id)
                }.Concat(columnsToReturn)
                 .Distinct()
                 .ToArray();
        }

        var filteredResult =
            entitiesQuery.Select(columnsToReturn.ToDtoSelectorExpression<TIn>());

        return filteredResult;
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
                                                                         Func<IQueryable<TIn>, IEnumerable<TIn>>
                                                                             postProcessing = null)
        where TIn : class
    {

        entitiesQuery = entitiesQuery.ApplyDeepFilters(filterDictionary);

        var totalRecords = DynamicQueryableExtensions.Count(entitiesQuery);

        var orderedQuery = entitiesQuery.AddOrderingAndPaginationQuery(filterDictionary,
                                                                       defaultSortColumnName,
                                                                       out var pageSize,
                                                                       out var pageIndex,
                                                                       postProcessing);

        var finalQuery = orderedQuery.PerformColumnsSelectorQuery(columnsToPick);

        return controller.GetPagedListResult(finalQuery, totalRecords, pageIndex, pageSize);
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