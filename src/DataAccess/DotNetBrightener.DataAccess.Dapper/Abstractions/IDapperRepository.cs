namespace DotNetBrightener.DataAccess.Dapper.Abstractions;

public interface IDapperRepository
{
    Task<IQueryable<TEntity>> FetchEntities<TEntity>(string sqlQuery, object param = null);

    Task<TEntity> GetEntity<TEntity>(string sqlQuery, object param = null);

    Task<TEntity> ExecuteScalar<TEntity>(string sqlQuery, object param = null);

    /// <summary>
    ///     Executes a non-query SQL statement (insert/update/delete/ddl) and returns the number of affected rows.
    /// </summary>
    Task<int> ExecuteAsync(string sqlQuery, object param = null);

    /// <summary>
    ///     Opens a single connection and starts a database transaction that the returned unit of work's
    ///     methods run against. Disposing without calling <see cref="IDapperUnitOfWork.CommitAsync"/> rolls back.
    /// </summary>
    Task<IDapperUnitOfWork> BeginUnitOfWorkAsync();
}