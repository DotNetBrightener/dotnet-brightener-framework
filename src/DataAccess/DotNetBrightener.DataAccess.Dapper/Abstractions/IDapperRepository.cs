namespace DotNetBrightener.DataAccess.Dapper.Abstractions;

public interface IDapperRepository
{
    Task<IQueryable<TEntity>> FetchEntities<TEntity>(string sqlQuery, object param = null);

    Task<TEntity> GetEntity<TEntity>(string sqlQuery, object param = null);

    Task<TEntity> ExecuteScalar<TEntity>(string sqlQuery, object param = null);
}