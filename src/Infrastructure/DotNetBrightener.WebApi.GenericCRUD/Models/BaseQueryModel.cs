using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.WebApi.GenericCRUD.Models;

/// <summary>
///     Represents the default query model that can be read by the API to apply properties filter, ordering and pagination queries
/// </summary>
public class BaseQueryModel
{
    /// <summary>
    ///     Gets or sets the array of strings represents the properties of the entity to retrieve when making the REST API request
    /// </summary>
    public string Columns
    {
        get => string.Join(";", FilteredColumns);
        set
        {
            FilteredColumns = string.IsNullOrEmpty(value)
                                  ? Array.Empty<string>()
                                  : value.Split(new [ ]
                                                {
                                                    ',', ';'
                                                },
                                                StringSplitOptions.RemoveEmptyEntries);
        }
    }

    /// <summary>
    ///     Gets or sets the number of items to return in the response for paged data
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    ///     Gets or sets the 0-based index of current page being requested for the List request
    /// </summary>
    public int PageIndex { get; set; } = 0;

    /// <summary>
    ///     Gets or sets the columns that are used to sort the collection of the result items
    /// </summary>
    public string OrderBy
    {
        get => string.Join(";", OrderedColumns);
        set
        {
            OrderedColumns = string.IsNullOrEmpty(value)
                                 ? Array.Empty<string>()
                                 : value.Split(new [ ]
                                               {
                                                   ',', ';'
                                               },
                                               StringSplitOptions.RemoveEmptyEntries);
        }
    }

    /// <summary>
    ///     Retrieves the collection of columns (properties) of the entity to retrieve from the REST API request
    /// </summary>
    public string [ ] FilteredColumns { get; private set; } = Array.Empty<string>();

    /// <summary>
    ///     Retrieves the collection of columns (properties) of the entity to retrieve from the REST API request
    /// </summary>
    public string [ ] OrderedColumns { get; private set; } = Array.Empty<string>();

    public bool DeletedRecordsOnly { get; set; } = false;

    public static BaseQueryModel FromQuery(IQueryCollection query)
    {
        var queryDictionary = query.ToDictionary(_ => _.Key,
                                                 _ => _.Value.ToString());

        return queryDictionary.ToQueryModel<BaseQueryModel>();
    }

    public static TOutputModel FromQuery<TOutputModel>(IQueryCollection query)
        where TOutputModel : BaseQueryModel, new()
    {
        var queryDictionary = query.ToDictionary(_ => _.Key,
                                                 _ => _.Value.ToString());

        return queryDictionary.ToQueryModel<TOutputModel>();
    }
}