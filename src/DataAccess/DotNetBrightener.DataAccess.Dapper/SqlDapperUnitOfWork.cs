using System.Data;
using Dapper;
using DotNetBrightener.DataAccess.Dapper.Abstractions;

namespace DotNetBrightener.DataAccess.Dapper;

public class SqlDapperUnitOfWork(IDbConnection connection) : IDapperUnitOfWork
{
    private readonly IDbTransaction _transaction = connection.BeginTransaction();

    private bool _completed;

    public async Task<IQueryable<TEntity>> FetchEntities<TEntity>(string sqlQuery, object param = null)
    {
        var result = await connection.QueryAsync<TEntity>(sqlQuery, param, _transaction);

        return result.AsQueryable();
    }

    public Task<TEntity> GetEntity<TEntity>(string sqlQuery, object param = null) =>
        connection.QueryFirstOrDefaultAsync<TEntity>(sqlQuery, param, _transaction);

    public Task<TEntity> ExecuteScalar<TEntity>(string sqlQuery, object param = null) =>
        connection.ExecuteScalarAsync<TEntity>(sqlQuery, param, _transaction);

    public Task<int> ExecuteAsync(string sqlQuery, object param = null) =>
        connection.ExecuteAsync(sqlQuery, param, _transaction);

    public Task CommitAsync()
    {
        _transaction.Commit();
        _completed = true;

        return Task.CompletedTask;
    }

    public Task RollbackAsync()
    {
        _transaction.Rollback();
        _completed = true;

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (!_completed)
        {
            _transaction.Rollback();
        }

        _transaction.Dispose();
        connection.Dispose();

        return ValueTask.CompletedTask;
    }
}
