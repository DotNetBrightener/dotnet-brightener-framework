namespace DotNetBrightener.DataAccess;

/// <summary>
///     Database providers supported by the library
/// </summary>
public enum DatabaseProvider
{
    /// <summary>
    ///     The MS SQL DBMS
    /// </summary>
    MsSql,

    /// <summary>
    ///     The PostgreSQL DBMS
    /// </summary>
    PostgreSql,

    /// <summary>
    ///     The Sqlite cross platform db
    /// </summary>
    Sqlite,
}