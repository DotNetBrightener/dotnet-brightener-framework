namespace DotNetBrightener.Caching;

/// <summary>
/// Represents default values related to caching
/// </summary>
public static class CacheDefaultSettings
{
    /// <summary>
    ///     Gets the default cache time in minutes
    /// </summary>
    public static int CacheTime => 60;

    /// <summary>
    ///     Gets a key for caching
    /// </summary>
    /// <remarks>
    /// {0} : Entity type name
    /// {1} : Entity id
    /// </remarks>
    public static string EntityCacheKey => "DNB.{0}.id-{1}";
}