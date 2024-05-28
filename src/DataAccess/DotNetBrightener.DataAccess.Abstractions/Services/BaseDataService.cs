using DotNetBrightener.DataAccess.Models;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using DotNetBrightener.DataAccess.Attributes;
using DotNetBrightener.DataAccess.Utils;

namespace DotNetBrightener.DataAccess.Services;

public abstract class BaseDataService<TEntity> : IBaseDataService<TEntity> where TEntity : class, new()
{
    protected readonly IRepository Repository;

    protected BaseDataService(IRepository repository)
    {
        Repository = repository;
    }

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

    public virtual void Insert(IEnumerable<TEntity> entities)
    {
        InsertManyAsync(entities).Wait();
    }

    public virtual async Task InsertAsync(TEntity entity)
    {
        Repository.Insert(entity);
        Repository.CommitChanges();
    }

    public virtual async Task InsertManyAsync(IEnumerable<TEntity> entities)
    {
        Repository.InsertMany(entities);
        Repository.CommitChanges();
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
        Repository.CommitChanges();
    }

    public virtual async Task<int> UpdateMany(Expression<Func<TEntity, bool>>?   filterExpression,
                                              Expression<Func<TEntity, TEntity>> updateExpression)
    {
        var affectedRecords = Repository.Update(filterExpression, updateExpression);
        Repository.CommitChanges();

        return affectedRecords;
    }

    public virtual async Task DeleteOne(Expression<Func<TEntity, bool>>? filterExpression,
                                        string                           reason          = null,
                                        bool                             forceHardDelete = false)
    {
        Repository.DeleteOne(filterExpression, reason, forceHardDelete);
        Repository.CommitChanges();
    }

    public virtual async Task RestoreOne(Expression<Func<TEntity, bool>>? filterExpression)
    {
        Repository.RestoreOne(filterExpression);
        Repository.CommitChanges();
    }

    public virtual async Task<int> DeleteMany(Expression<Func<TEntity, bool>>? filterExpression,
                                              string                           reason          = null,
                                              bool                             forceHardDelete = false)
    {
        int updatedRecords = Repository.DeleteMany(filterExpression, reason, forceHardDelete);
        Repository.CommitChanges();

        return updatedRecords;
    }

    public virtual async Task<int> RestoreMany(Expression<Func<TEntity, bool>> filterExpression)
    {
        var affectedRecords = Repository.RestoreMany(filterExpression);
        Repository.CommitChanges();

        return affectedRecords;
    }

    public void Dispose()
    {
        Repository.CommitChanges();
    }
}