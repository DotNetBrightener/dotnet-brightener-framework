using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNetBrightener.DataAccess.Models;

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

    public virtual IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>> expression = null)
    {
        return Repository.Fetch(expression);
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
        InsertAsync(entities).Wait();
    }

    public virtual async Task InsertAsync(TEntity entity)
    {
        Repository.Insert(entity);
        Repository.CommitChanges();
    }

    public virtual async Task InsertAsync(IEnumerable<TEntity> entities)
    {
        Repository.Insert<TEntity>(entities);
        Repository.CommitChanges();
    }

    public virtual void Update(TEntity entity)
    {
        Repository.Update(entity);
        Repository.CommitChanges();
    }

    public virtual void Update(IEnumerable<TEntity> entities)
    {
        Repository.Update(entities);
        Repository.CommitChanges();
    }

    public virtual void UpdateOne(Expression<Func<TEntity, bool>>    filterExpression,
                                  Expression<Func<TEntity, TEntity>> updateExpression)
    {
        Repository.Update(filterExpression, updateExpression, 1);
        Repository.CommitChanges();
    }

    public virtual void UpdateMany(Expression<Func<TEntity, bool>>    filterExpression,
                                   Expression<Func<TEntity, TEntity>> updateExpression)
    {
        Repository.Update(filterExpression, updateExpression);
        Repository.CommitChanges();
    }

    public virtual void DeleteOne(Expression<Func<TEntity, bool>> filterExpression, bool forceHardDelete = false)
    {
        Repository.DeleteOne(filterExpression, forceHardDelete);
        Repository.CommitChanges();
    }

    public virtual int DeleteMany(Expression<Func<TEntity, bool>> filterExpression, bool forceHardDelete = false)
    {
        int updatedRecords = Repository.DeleteMany(filterExpression, forceHardDelete);
        Repository.CommitChanges();

        return updatedRecords;
    }

    public void Dispose()
    {
        Repository.CommitChanges();
    }
}