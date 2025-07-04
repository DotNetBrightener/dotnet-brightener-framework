using System.Web;

// ReSharper disable CheckNamespace
namespace System;

internal static class UriBuilderExtensions
{
    public static UriBuilder AddQueryString(this UriBuilder uri, string queryName, string queryValue)
    {
        var query = HttpUtility.ParseQueryString(uri.Query);

        query[queryName] = queryValue;
        uri.Query        = query.ToString() ?? "";

        return uri;
    }

    public static UriBuilder AddQueryParameters(this UriBuilder             uri,
                                                IDictionary<string, string> queries)
    {
        var query = HttpUtility.ParseQueryString(uri.Query);

        foreach (var queryEntry in queries)
        {
            query[queryEntry.Key] = queryEntry.Value;
        }

        uri.Query = query.ToString() ?? "";

        return uri;
    }

    public static void RemoveQueryString(this UriBuilder uri, string queryName)
    {
        var query = HttpUtility.ParseQueryString(uri.Query);

        query[queryName] = null;
        uri.Query        = query.ToString() ?? "";
    }

    public static Uri RemoveQueryString(this Uri uri, string queryName)
    {
        var uriBuilder = new UriBuilder(uri);
        var query      = HttpUtility.ParseQueryString(uriBuilder.Query);

        query[queryName] = null;
        uriBuilder.Query = query.ToString() ?? "";

        return uriBuilder.Uri;
    }

    public static string GetBaseUrl(this Uri uri)
    {
        return uri.GetLeftPart(UriPartial.Authority);
    }

    public static string GetDomain(this Uri uri)
    {
        return uri.GetLeftPart(UriPartial.Authority)
                  .Replace("http://", "")
                  .Replace("https://", "");
    }
}