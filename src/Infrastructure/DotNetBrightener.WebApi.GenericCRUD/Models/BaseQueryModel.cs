﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
                                  : value.Split(new[]
                                                {
                                                    ',', ';'
                                                },
                                                StringSplitOptions.RemoveEmptyEntries);
        }
    }

    /// <summary>
    ///     Gets or sets the number of items to return in the response for paged data
    /// </summary>
    public int PageSize { get; set; } = 50;

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
                                 : value.Split(new[]
                                               {
                                                   ',', ';'
                                               },
                                               StringSplitOptions.RemoveEmptyEntries);
        }
    }

    /// <summary>
    ///     Retrieves the collection of columns (properties) of the entity to retrieve from the REST API request
    /// </summary>
    public string[] FilteredColumns { get; private set; } = Array.Empty<string>();

    /// <summary>
    ///     Retrieves the collection of columns (properties) of the entity to retrieve from the REST API request
    /// </summary>
    public string[] OrderedColumns { get; private set; } = Array.Empty<string>();

    public bool DeletedRecordsOnly { get; set; } = false;

    public Dictionary<string, string> QueryDictionary { get; private set; }

    [DebuggerStepThrough]
    public static BaseQueryModel FromQuery(IQueryCollection query)
    {
        var queryDictionary = query.ToDictionary(_ => _.Key,
                                                 _ => _.Value.ToString());


        return FromDictionary<BaseQueryModel>(queryDictionary);
    }

    [DebuggerStepThrough]
    internal static TOutputModel FromQuery<TOutputModel>(IQueryCollection query)
        where TOutputModel : BaseQueryModel, new()
    {
        var queryDictionary = query.ToDictionary(_ => _.Key,
                                                 _ => _.Value.ToString());

        return FromDictionary<TOutputModel>(queryDictionary);
    }

    [DebuggerStepThrough]
    internal static TOutputModel FromDictionary<TOutputModel>(Dictionary<string, string> queryDictionary)
        where TOutputModel : BaseQueryModel, new()
    {
        var baseQueryModel = ToQueryModel<TOutputModel>(queryDictionary);

        baseQueryModel.QueryDictionary = queryDictionary;

        return baseQueryModel;
    }
    
    /// <summary>
    ///     Convert the dictionary of string: string to the query model of <typeparamref name="TQueryModel" />
    /// </summary>
    /// <typeparam name="TQueryModel">Type of the model to convert to</typeparam>
    /// <returns>
    ///     The instance of <typeparamref name="TQueryModel" /> from the provided dictionary
    /// </returns>
    public static TQueryModel ToQueryModel<TQueryModel>(Dictionary<string, string> queryDictionary)
    {
        var queryModel = Activator.CreateInstance<TQueryModel>();

        if (queryDictionary == null ||
            !queryDictionary.Any())
        {
            return queryModel;
        }

        var targetType = typeof(TQueryModel);

        foreach (var keyValuePair in queryDictionary)
        {
            if (targetType.RetrieveMemberInfo(keyValuePair.Key) is PropertyInfo property)
            {
                var propertyValue = Convert.ChangeType(keyValuePair.Value, property.PropertyType);
                property.SetValue(queryModel, propertyValue);
            }
        }

        return queryModel;
    }
}

public static class BaseQueryModelExtensions
{
    [DebuggerStepThrough]
    public static TOutputModel ToQueryModel<TOutputModel>(this Dictionary<string, string> queryDictionary)
        where TOutputModel : BaseQueryModel, new()
    {
        var baseQueryModel = BaseQueryModel.FromDictionary<TOutputModel>(queryDictionary);

        return baseQueryModel;
    }

    [DebuggerStepThrough]
    public static TOutputModel ToQueryModel<TOutputModel>(this IQueryCollection query)
        where TOutputModel : BaseQueryModel, new()
    {
        var baseQueryModel = BaseQueryModel.FromQuery<TOutputModel>(query);

        return baseQueryModel;
    }
}