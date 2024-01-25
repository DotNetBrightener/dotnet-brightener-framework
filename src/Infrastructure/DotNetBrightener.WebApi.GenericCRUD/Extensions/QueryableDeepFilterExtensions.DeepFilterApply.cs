﻿using DotNetBrightener.DataAccess.Models;
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
                                                              string[] columnsToPick = null) where TIn : class
    {
        var alwaysIgnoreColumns = typeof(TIn).GetIgnoredProperties();

        if (columnsToPick is null ||
            columnsToPick.Length == 0)
        {
            var availableColumns = typeof(TIn).GetDefaultColumns();
            columnsToPick = availableColumns.Except(alwaysIgnoreColumns)
                                            .Distinct()
                                            .ToArray();
        }

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
                                                                         string[] columnsToPick)
        where TIn : class
    {

        entitiesQuery = entitiesQuery.ApplyDeepFilters(filterDictionary);

        var totalRecords = DynamicQueryableExtensions.Count(entitiesQuery);

        var orderedQuery = entitiesQuery.AddOrderingAndPaginationQuery(filterDictionary,
                                                                       defaultSortColumnName,
                                                                       out var pageSize,
                                                                       out var pageIndex);

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

        responseHeaders.Append("Result-Totals", totalRecords.ToString());
        responseHeaders.Append("Result-PageIndex", pageIndex.ToString());
        responseHeaders.Append("Result-PageSize", pageSize.ToString());

        var result = finalQuery.ToDynamicArray();
        responseHeaders.Append("Result-Count", result.Length.ToString());

        // add expose header to support CORS
        responseHeaders.Append("Access-Control-Expose-Headers",
                               "Result-Totals,Result-PageIndex,Result-PageSize,Result-Count," +
                               "Result-Totals,Result-PageIndex,Result-PageSize,Result-Count".ToLower());

        return controller.Ok(result);
    }
}