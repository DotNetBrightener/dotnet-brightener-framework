using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNetBrightener.Core.DataAccess.Repositories;

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