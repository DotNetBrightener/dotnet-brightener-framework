using System.Web;

// ReSharper disable once CheckNamespace
namespace System;

public static class UriBuilderExtensions
{
    public static void AddQueryString(this UriBuilder uri, string queryName, string queryValue)
    {
        var query = HttpUtility.ParseQueryString(uri.Query);

        query [queryName] = queryValue;
        uri.Query         = query.ToString() ?? "";
    }

    public static Uri AddQueryString(this Uri uri, string queryName, string queryValue)
    {
        var uriBuilder = new UriBuilder(uri);
        var query      = HttpUtility.ParseQueryString(uriBuilder.Query);

        query [queryName] = queryValue;
        uriBuilder.Query  = query.ToString() ?? "";

        return uriBuilder.Uri;
    }

    public static void RemoveQueryString(this UriBuilder uri, string queryName)
    {
        var query = HttpUtility.ParseQueryString(uri.Query);

        query [queryName] = null;
        uri.Query         = query.ToString() ?? "";
    }

    public static Uri RemoveQueryString(this Uri uri, string queryName)
    {
        var uriBuilder = new UriBuilder(uri);
        var query      = HttpUtility.ParseQueryString(uriBuilder.Query);

        query [queryName] = null;
        uriBuilder.Query  = query.ToString() ?? "";

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