using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace DotNetBrightener.WebApi.GenericCRUD.Extensions;

public static class CorsPolicyBuilderExtensions
{
    public static CorsPolicyBuilder AddPagedDataSetExposedHeaders(this CorsPolicyBuilder builder)
    {
        builder.WithExposedHeaders(HeaderDictionaryExtensions.PagedListResultExposedHeaders);

        return builder;
    }
}

internal static class HeaderDictionaryExtensions
{
    internal static readonly string[] HeaderKeysForTotalCount =
    [
        "X-Total-Count",
        "Result-Totals"
    ];

    internal static readonly string[] HeaderKeysForResultCount =
    [
        "X-Result-Count",
        "Result-Count"
    ];

    internal static readonly string[] HeaderKeysForPageSize =
    [
        "X-Page-Size",
        "Result-PageSize"
    ];

    internal static readonly string[] HeaderKeysForPageIndex =
    [
        "X-Page-Index",
        "Result-PageIndex"
    ];

    internal static readonly string[] PagedListResultExposedHeaders =
    [
        ..HeaderKeysForTotalCount,
        ..HeaderKeysForResultCount,
        ..HeaderKeysForPageSize,
        ..HeaderKeysForPageIndex
    ];

    public static IHeaderDictionary AppendTotalCount(this IHeaderDictionary headers, int totalCount)
    {
        foreach (var headerKey in HeaderKeysForTotalCount)
            headers.Append(headerKey, totalCount.ToString());

        //headers.Append("Access-Control-Expose-Headers", HeaderKeysForTotalCount);

        return headers;
    }

    public static IHeaderDictionary AppendResultCount(this IHeaderDictionary headers, int totalCount)
    {
        foreach (var headerKey in HeaderKeysForResultCount)
            headers.Append(headerKey, totalCount.ToString());

        //headers.Append("Access-Control-Expose-Headers", HeaderKeysForResultCount);

        return headers;
    }

    public static IHeaderDictionary AppendPageSize(this IHeaderDictionary headers, int totalCount)
    {
        foreach (var headerKey in HeaderKeysForPageSize)
            headers.Append(headerKey, totalCount.ToString());

        //headers.Append("Access-Control-Expose-Headers", HeaderKeysForPageSize);

        return headers;
    }

    public static IHeaderDictionary AppendPageIndex(this IHeaderDictionary headers, int totalCount)
    {
        foreach (var headerKey in HeaderKeysForPageIndex)
            headers.Append(headerKey, totalCount.ToString());

        //headers.Append("Access-Control-Expose-Headers", HeaderKeysForPageIndex);

        return headers;
    }
}