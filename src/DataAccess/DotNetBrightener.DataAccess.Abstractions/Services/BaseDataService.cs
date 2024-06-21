#nullable enable
using DotNetBrightener.DataAccess.Models;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace DotNetBrightener.DataAccess.Services;

public abstract class BaseDataService<TEntity>(IRepository repository) : IBaseDataService<TEntity>
    where TEntity : class, new()
{
    protected readonly IRepository Repository = repository;

    public virtual TEntity? Get(Expression<Func<TEntity, bool>> expression)
    {
        return Repository.Get(expression);
    }

    public IQueryable<TEntity> FetchHistory(Expression<Func<TEntity, bool>>? expression = null,
                                            DateTimeOffset?                  from       = null,
                                            DateTimeOffset?                  to         = null)
    {
        var query = Repository.FetchHistory(expression, from, to);

        return query;
    }

    public virtual IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>>? expression = null)
    {
        return Repository.Fetch(expression);
    }

    public IQueryable<TEntity> FetchDeletedRecords(Expression<Func<TEntity, bool>>? expression = null)
    {
        IQueryable<TEntity> query = Repository.Fetch(expression);

        if (typeof(TEntity).HasProperty<bool>(nameof(IAuditableEntity.IsDeleted)))
        {
            query = query.Where("IsDeleted == True");
        }

        return query;
    }

    public IQueryable<TEntity> FetchActive(Expression<Func<TEntity, bool>>? expression = null)
    {
        IQueryable<TEntity> query = Repository.Fetch(expression);

        if (typeof(TEntity).HasProperty<bool>(nameof(IAuditableEntity.IsDeleted)))
        {
            query = query.Where("IsDeleted != True");
        }

        return query;
    }

    public virtual void Insert(TEntity entity)
    {
        InsertAsync(entity).Wait();
    }

    public virtual void InsertMany(IEnumerable<TEntity> entities)
    {
        InsertManyAsync(entities).Wait();
    }

    public virtual void BulkInsert(IEnumerable<TEntity> entities)
    {
        BulkInsertAsync(entities).Wait();
    }

    public virtual async Task InsertAsync(TEntity entity)
    {
        await Repository.InsertAsync(entity);
        await Repository.CommitChangesAsync();
    }

    public virtual async Task InsertManyAsync(IEnumerable<TEntity> entities)
    {
        await Repository.InsertManyAsync(entities);
        await Repository.CommitChangesAsync();
    }


    public virtual async Task BulkInsertAsync(IEnumerable<TEntity> entities)
    {
        await Repository.BulkInsertAsync(entities);
        await Repository.CommitChangesAsync();
    }

    public virtual void Update(TEntity entity)
    {
        Repository.Update(entity);
        Repository.CommitChanges();
    }

    public virtual void Update(TEntity entity, object dto)
    {
        Repository.Update(entity, dto);
        Repository.CommitChanges();
    }

    public virtual void UpdateMany(params TEntity[] entities)
    {
        Repository.UpdateMany(entities);
        Repository.CommitChanges();
    }

    public virtual async Task UpdateOne(Expression<Func<TEntity, bool>>?   filterExpression,
                                        Expression<Func<TEntity, TEntity>> updateExpression)
    {
        Repository.Update(filterExpression, updateExpression, 1);
        await Repository.CommitChangesAsync();
    }

    public virtual async Task<int> UpdateMany(Expression<Func<TEntity, bool>>?   filterExpression,
                                              Expression<Func<TEntity, TEntity>> updateExpression)
    {
        var affectedRecords = Repository.Update(filterExpression, updateExpression);
        await Repository.CommitChangesAsync();

        return affectedRecords;
    }

    public virtual async Task DeleteOne(Expression<Func<TEntity, bool>>? filterExpression,
                                        string?                          reason          = null,
                                        bool                             forceHardDelete = false)
    {
        Repository.DeleteOne(filterExpression, reason, forceHardDelete);
        await Repository.CommitChangesAsync();
    }

    public virtual async Task RestoreOne(Expression<Func<TEntity, bool>>? filterExpression)
    {
        Repository.RestoreOne(filterExpression);
        await Repository.CommitChangesAsync();
    }

    public virtual async Task<int> DeleteMany(Expression<Func<TEntity, bool>>? filterExpression,
                                              string?                          reason          = null,
                                              bool                             forceHardDelete = false)
    {
        int updatedRecords = Repository.DeleteMany(filterExpression, reason, forceHardDelete);
        await Repository.CommitChangesAsync();

        return updatedRecords;
    }

    public virtual async Task<int> RestoreMany(Expression<Func<TEntity, bool>>? filterExpression)
    {
        var affectedRecords = Repository.RestoreMany(filterExpression);
        await Repository.CommitChangesAsync();

        return affectedRecords;
    }

    public void Dispose()
    {
        Repository.CommitChanges();
    }
}