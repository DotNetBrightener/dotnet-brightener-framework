using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNetBrightener.Core.DataAccess.Repositories;

namespace DotNetBrightener.SharedDataAccessService
{
    public interface IBaseDataService<TEntity>
    {
        TEntity Get(Expression<Func<TEntity, bool>> expression);

        IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>> expression = null);

        void Insert(TEntity entity);

        Task InsertAsync(TEntity entity);

        void UpdateOne(Expression<Func<TEntity, bool>> filterExpression,
                       Expression<Func<TEntity, TEntity>> updateExpression);

        void UpdateMany(Expression<Func<TEntity, bool>> filterExpression,
                        Expression<Func<TEntity, TEntity>> updateExpression);

        void DeleteOne(Expression<Func<TEntity, bool>> filterExpression, bool forceHardDelete = false);

        void DeleteMany(Expression<Func<TEntity, bool>> filterExpression, bool forceHardDelete = false);
    }

    public abstract class BaseDataService<TEntity> : IBaseDataService<TEntity> where TEntity : class, new()
    {
        protected readonly IBaseRepository Repository;

        protected BaseDataService(IBaseRepository repository)
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

        public virtual void Insert(TEntity entity)
        {
            InsertAsync(entity).Wait();
        }

        public virtual async Task InsertAsync(TEntity entity)
        {
            await Repository.Insert(entity);
        }

        public virtual void UpdateOne(Expression<Func<TEntity, bool>> filterExpression,
                                      Expression<Func<TEntity, TEntity>> updateExpression)
        {
            Repository.Update(filterExpression, updateExpression, 1);
        }

        public virtual void UpdateMany(Expression<Func<TEntity, bool>> filterExpression,
                                       Expression<Func<TEntity, TEntity>> updateExpression)
        {
            Repository.Update(filterExpression, updateExpression);
        }

        public virtual void DeleteOne(Expression<Func<TEntity, bool>> filterExpression, bool forceHardDelete = false)
        {
            Repository.DeleteOne(filterExpression, forceHardDelete);
        }

        public virtual void DeleteMany(Expression<Func<TEntity, bool>> filterExpression, bool forceHardDelete = false)
        {
            Repository.DeleteMany(filterExpression, forceHardDelete).Wait();
        }
    }
}