using DotNetBrightener.Caching;
using DotNetBrightener.Core.DataAccess.Abstractions.Repositories;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DotNetBrightener.SharedDataAccessService
{
    public interface IBaseDataService<TEntity>
    {
        /// <summary>
        ///     Returns a specific record that matches the given expression
        /// </summary>
        /// <param name="expression">The condition to fetch the record</param>
        /// <returns>The entity record, if matched, otherwise, <c>null</c></returns>
        TEntity Get(Expression<Func<TEntity, bool>> expression);

        /// <summary>
        ///     Generates a fetch query to the given entity table, optionally provides the condition for filtering
        /// </summary>
        /// <param name="expression">The condition for filtering records in the query</param>
        /// <returns>An IQueryable of the collection of the requested entities</returns>
        IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>> expression = null);


        /// <summary>
        ///     Generates a fetch query to the given entity table with the non-deleted records, optionally provides the condition for filtering
        /// </summary>
        /// <param name="expression">The condition for filtering records in the query</param>
        /// <returns>An IQueryable of the collection of the requested entities, which are not deleted</returns>
        IQueryable<TEntity> FetchActive(Expression<Func<TEntity, bool>> expression = null);

        /// <summary>
        ///     Inserts a new record of the entity to the database
        /// </summary>
        /// <param name="entity">The record to insert</param>
        void Insert(TEntity entity);

        /// <summary>
        ///     Inserts a new record of the entity to the database
        /// </summary>
        /// <param name="entity">The record to insert</param>
        Task InsertAsync(TEntity entity);

        /// <summary>
        ///     Update the matched record with the given filter expression, expected only one record affected
        /// </summary>
        /// <param name="filterExpression">The expression for selecting the record to update</param>
        /// <param name="updateExpression">The expression that describes the update instruction</param>
        void UpdateOne(Expression<Func<TEntity, bool>> filterExpression,
                       Expression<Func<TEntity, TEntity>> updateExpression);

        /// <summary>
        ///     Update multiple matched records with the given filter expression
        /// </summary>
        /// <param name="filterExpression">The expression for selecting the records to update</param>
        /// <param name="updateExpression">The expression that describes the update instruction</param>
        void UpdateMany(Expression<Func<TEntity, bool>> filterExpression,
                        Expression<Func<TEntity, TEntity>> updateExpression);

        /// <summary>
        ///     Delete the matched record with the given filter expression, expected only one record affected
        /// </summary>
        /// <param name="filterExpression">
        ///     The expression for selecting the record to delete
        /// </param>
        /// <param name="forceHardDelete">
        ///     Indicates whether the deletion is permanent. Default is <c>False</c> which marks the record as deleted
        /// </param>
        void DeleteOne(Expression<Func<TEntity, bool>> filterExpression, bool forceHardDelete = false);


        /// <summary>
        ///     Delete multiple matched records with the given filter expression
        /// </summary>
        /// <param name="filterExpression">
        ///     The expression for selecting the records to delete
        /// </param>
        /// <param name="forceHardDelete">
        ///     Indicates whether the deletion is permanent. Default is <c>False</c> which marks the records as deleted
        /// </param>
        void DeleteMany(Expression<Func<TEntity, bool>> filterExpression, bool forceHardDelete = false);
    }

    public abstract class BaseDataService<TEntity> : IBaseDataService<TEntity> where TEntity : class, new()
    {
        protected readonly IBaseRepository Repository;
        protected readonly ICacheManager CacheManager;

        /// <summary>
        ///     Indicates the data access service should use cache. Default is <c>true</c>
        /// </summary>
        protected virtual bool UseCache => true;

        /// <summary>
        ///     Specifies the time in minute for the cache to be kept
        /// </summary>
        protected virtual int DefaultCacheTime => 20;

        protected BaseDataService(IBaseRepository repository, 
                                  ICacheManager cacheManager)
        {
            Repository = repository;
            CacheManager = cacheManager;
        }

        public virtual TEntity Get(Expression<Func<TEntity, bool>> expression)
        {
            if (!UseCache)
            {
                return Repository.Get(expression);
            }

            var result = CacheManager.Get(GetCacheKey(expression), () =>
            {
                return Repository.Get(expression);
            });

            return result;
        }

        public virtual IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>> expression = null)
        {
            return Repository.Fetch(expression);
        }

        public IQueryable<TEntity> FetchActive(Expression<Func<TEntity, bool>> expression = null)
        {
            IQueryable<TEntity> query = Repository.Fetch(expression);

            if (typeof(TEntity).HasProperty<bool>("IsDeleted"))
            {
                query = query.Where("IsDeleted != True");
            }

            return query;
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
            CacheManager.Remove(GetCacheKey(filterExpression));
        }

        public virtual void UpdateMany(Expression<Func<TEntity, bool>> filterExpression,
                                       Expression<Func<TEntity, TEntity>> updateExpression)
        {
            Repository.Update(filterExpression, updateExpression);
        }

        public virtual void DeleteOne(Expression<Func<TEntity, bool>> filterExpression, bool forceHardDelete = false)
        {
            Repository.DeleteOne(filterExpression, forceHardDelete);
            CacheManager.Remove(GetCacheKey(filterExpression));
        }

        public virtual void DeleteMany(Expression<Func<TEntity, bool>> filterExpression, bool forceHardDelete = false)
        {
            Repository.DeleteMany(filterExpression, forceHardDelete).Wait();
        }

        /// <summary>
        ///     Retrieves the cache key for the cache from given query expression
        /// </summary>
        /// <param name="queryExpression">
        ///     The expression describes how to fetch the record
        /// </param>
        /// <param name="cacheTime">
        ///     Indicates the amount of time the cache should live
        /// </param>
        /// <returns>
        ///     The <see cref="CacheKey" /> object
        /// </returns>
        protected virtual CacheKey GetCacheKey(Expression<Func<TEntity, bool>> queryExpression, int? cacheTime = null)
        {
            if (cacheTime == null)
            {
                cacheTime = DefaultCacheTime;
            }

            var cacheKeyString = queryExpression.GenerateCacheKey();

            return new CacheKey(cacheKeyString, cacheTime: cacheTime, typeof(TEntity).Name);
        }
    }
}