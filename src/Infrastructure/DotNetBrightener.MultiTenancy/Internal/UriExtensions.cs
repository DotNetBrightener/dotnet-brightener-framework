// ReSharper disable once CheckNamespace
namespace System;
public static class UriExtensions
{
    public static string GetDomain(this Uri uri)
    {
        return uri.GetLeftPart(UriPartial.Authority)
                  .Replace("http://", "")
                  .Replace("https://", "");
    }
}
