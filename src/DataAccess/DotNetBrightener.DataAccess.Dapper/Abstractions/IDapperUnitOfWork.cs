namespace DotNetBrightener.DataAccess.Dapper.Abstractions;

/// <summary>
///     A single connection + transaction that Dapper statements can be run against together.
///     Dispose without calling <see cref="CommitAsync"/> to roll back.
/// </summary>
public interface IDapperUnitOfWork : IAsyncDisposable
{
    Task<IQueryable<TEntity>> FetchEntities<TEntity>(string sqlQuery, object param = null);

    Task<TEntity> GetEntity<TEntity>(string sqlQuery, object param = null);

    Task<TEntity> ExecuteScalar<TEntity>(string sqlQuery, object param = null);

    Task<int> ExecuteAsync(string sqlQuery, object param = null);

    Task CommitAsync();

    Task RollbackAsync();
}
