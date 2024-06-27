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

    public virtual async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> expression)
    {
        return await Repository.GetAsync(expression);
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

    public virtual IQueryable<TResult> Fetch<TResult>(Expression<Func<TEntity, bool>>?   expression,
                                                      Expression<Func<TEntity, TResult>> propertiesPickupExpression)
    {
        return Repository.Fetch(expression, propertiesPickupExpression);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? expression = null)
    {
        return await Repository.CountAsync(expression);
    }

    public virtual async Task<int> CountNonDeletedAsync(Expression<Func<TEntity, bool>>? expression = null)
    {
        return await Repository.CountNonDeletedAsync(expression);
    }

    public IQueryable<TEntity> FetchDeletedRecords(Expression<Func<TEntity, bool>>? expression = null)
    {
        if (!typeof(TEntity).HasProperty<bool>(nameof(IAuditableEntity.IsDeleted)))
        {
            throw new
                InvalidOperationException($"Entity of type {typeof(TEntity).Name} does not have soft-delete capability");
        }
        
        return Repository.Fetch(expression)
                         .Where($"{nameof(IAuditableEntity.IsDeleted)} == True");
    }

    public IQueryable<TEntity> FetchActive(Expression<Func<TEntity, bool>>? expression = null)
        => FetchNonDeleted(expression);

    public IQueryable<TEntity> FetchNonDeleted(Expression<Func<TEntity, bool>>? expression = null)
    {
        IQueryable<TEntity> query = Repository.Fetch(expression);

        if (typeof(TEntity).HasProperty<bool>(nameof(IAuditableEntity.IsDeleted)))
        {
            query = query.Where($"{nameof(IAuditableEntity.IsDeleted)} != True");
        }

        return query;
    }

    public virtual void Insert(TEntity entity) => InsertAsync(entity).Wait();

    public virtual void InsertMany(IEnumerable<TEntity> entities) => InsertManyAsync(entities).Wait();

    public virtual void BulkInsert(IEnumerable<TEntity> entities) => BulkInsertAsync(entities).Wait();

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

    public void Update(TEntity entity) => UpdateAsync(entity).Wait();

    public virtual async Task UpdateAsync(TEntity entity)
    {
        await Repository.UpdateAsync(entity);
        await Repository.CommitChangesAsync();
    }

    public void Update(TEntity entity, object dto, params string[] propertiesToIgnoreUpdate)
        => UpdateAsync(entity, dto, propertiesToIgnoreUpdate).Wait();

    public virtual async Task UpdateAsync(TEntity entity, object dto, params string[] propertiesToIgnoreUpdate)
    {
        await Repository.UpdateAsync(entity, dto, propertiesToIgnoreUpdate);
        await Repository.CommitChangesAsync();
    }

    public virtual void UpdateMany(params TEntity[] entities)
    {
        Repository.UpdateMany(entities);
        Repository.CommitChanges();
    }

    public virtual async Task UpdateOne(Expression<Func<TEntity, bool>>?   filterExpression,
                                        Expression<Func<TEntity, TEntity>> updateExpression)
    {
        await Repository.UpdateAsync(filterExpression, updateExpression, 1);
        await Repository.CommitChangesAsync();
    }

    public virtual async Task<int> UpdateMany(Expression<Func<TEntity, bool>>?   filterExpression,
                                              Expression<Func<TEntity, TEntity>> updateExpression)
    {
        var affectedRecords = await Repository.UpdateAsync(filterExpression, updateExpression);
        await Repository.CommitChangesAsync();

        return affectedRecords;
    }

    public virtual async Task DeleteOne(Expression<Func<TEntity, bool>>? filterExpression,
                                        string?                          reason          = null,
                                        bool                             forceHardDelete = false)
    {
        await Repository.DeleteOneAsync(filterExpression, reason, forceHardDelete);
        await Repository.CommitChangesAsync();
    }

    public virtual async Task RestoreOne(Expression<Func<TEntity, bool>>? filterExpression)
    {
        await Repository.RestoreOneAsync(filterExpression);
        await Repository.CommitChangesAsync();
    }

    public virtual async Task<int> DeleteMany(Expression<Func<TEntity, bool>>? filterExpression,
                                              string?                          reason          = null,
                                              bool                             forceHardDelete = false)
    {
        int updatedRecords = await Repository.DeleteManyAsync(filterExpression, reason, forceHardDelete);
        await Repository.CommitChangesAsync();

        return updatedRecords;
    }

    public virtual async Task<int> RestoreMany(Expression<Func<TEntity, bool>>? filterExpression)
    {
        var affectedRecords = await Repository.RestoreManyAsync(filterExpression);
        await Repository.CommitChangesAsync();

        return affectedRecords;
    }

    public void Dispose()
    {
        Repository.CommitChanges();
    }
}