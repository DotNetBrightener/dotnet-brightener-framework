using DotNetBrightener.DataAccess.Models;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace DotNetBrightener.DataAccess.Services;

public abstract class BaseDataService<TEntity> : IBaseDataService<TEntity> where TEntity : class, new()
{
    protected readonly IRepository Repository;

    protected BaseDataService(IRepository repository)
    {
        Repository = repository;
    }

    public virtual TEntity Get(Expression<Func<TEntity, bool>> expression)
    {
        return Repository.Get(expression);
    }

    public IQueryable<TEntity> FetchHistory(Expression<Func<TEntity, bool>> expression = null,
                                            DateTimeOffset?                 from       = null,
                                            DateTimeOffset?                 to         = null)
    {
        var query = Repository.FetchHistory(expression, from, to);

        return query;
    }

    public virtual IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>> expression = null)
    {
        return Repository.Fetch(expression);
    }

    public IQueryable<TEntity> FetchDeletedRecords(Expression<Func<TEntity, bool>> expression = null)
    {
        IQueryable<TEntity> query = Repository.Fetch(expression);

        if (typeof(TEntity).HasProperty<bool>(nameof(BaseEntityWithAuditInfo.IsDeleted)))
        {
            query = query.Where("IsDeleted == True");
        }

        return query;
    }

    public IQueryable<TEntity> FetchActive(Expression<Func<TEntity, bool>> expression = null)
    {
        IQueryable<TEntity> query = Repository.Fetch(expression);

        if (typeof(TEntity).HasProperty<bool>(nameof(BaseEntityWithAuditInfo.IsDeleted)))
        {
            query = query.Where("IsDeleted != True");
        }

        return query;
    }

    public virtual void Insert(TEntity entity)
    {
        InsertAsync(entity).Wait();
    }

    public virtual void Insert(IEnumerable<TEntity> entities)
    {
        InsertManyAsync(entities).Wait();
    }

    public virtual async Task InsertAsync(TEntity entity)
    {
        await Repository.Insert(entity);
        Repository.CommitChanges();
    }

    public virtual async Task InsertManyAsync(IEnumerable<TEntity> entities)
    {
        await Repository.InsertMany(entities);
        Repository.CommitChanges();
    }

    public virtual void Update(TEntity entity)
    {
        Repository.Update(entity);
        Repository.CommitChanges();
    }

    public virtual void UpdateMany(IEnumerable<TEntity> entities)
    {
        Repository.UpdateMany(entities);
        Repository.CommitChanges();
    }

    public virtual async Task UpdateOne(Expression<Func<TEntity, bool>>    filterExpression,
                                        Expression<Func<TEntity, TEntity>> updateExpression)
    {
        await Repository.Update(filterExpression, updateExpression, 1);
        Repository.CommitChanges();
    }

    public virtual async Task<int> UpdateMany(Expression<Func<TEntity, bool>>    filterExpression,
                                              Expression<Func<TEntity, TEntity>> updateExpression)
    {
        var affectedRecords = await Repository.Update(filterExpression, updateExpression);
        Repository.CommitChanges();

        return affectedRecords;
    }

    public virtual async Task DeleteOne(Expression<Func<TEntity, bool>> filterExpression,
                                        string                          reason          = null,
                                        bool                            forceHardDelete = false)
    {
        await Repository.DeleteOne(filterExpression, reason, forceHardDelete);
        Repository.CommitChanges();
    }

    public virtual async Task RestoreOne(Expression<Func<TEntity, bool>> filterExpression)
    {
        await Repository.RestoreOne(filterExpression);
        Repository.CommitChanges();
    }

    public virtual async Task<int> DeleteMany(Expression<Func<TEntity, bool>> filterExpression,
                                              string                          reason          = null,
                                              bool                            forceHardDelete = false)
    {
        int updatedRecords = await Repository.DeleteMany(filterExpression, reason, forceHardDelete);
        Repository.CommitChanges();

        return updatedRecords;
    }

    public virtual async Task<int> RestoreMany(Expression<Func<TEntity, bool>> filterExpression)
    {
        var affectedRecords = await Repository.RestoreMany(filterExpression);
        Repository.CommitChanges();

        return affectedRecords;
    }

    public void Dispose()
    {
        Repository.CommitChanges();
    }
}