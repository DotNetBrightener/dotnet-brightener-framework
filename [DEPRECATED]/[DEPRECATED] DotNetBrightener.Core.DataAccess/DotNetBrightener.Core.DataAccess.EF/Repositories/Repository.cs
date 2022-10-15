using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNetBrightener.Core.DataAccess.Abstractions.Repositories;
using DotNetBrightener.Core.DataAccess.Abstractions.Resolvers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DotNetBrightener.Core.DataAccess.EF.Repositories
{
    public abstract class Repository<TDbContext> : IRepository where TDbContext : DbContext
    {
        protected readonly TDbContext                   DbContext;
        protected readonly ICurrentLoggedInUserResolver CurrentUserResolver;

        protected Repository(TDbContext                   dbContext,
                             ICurrentLoggedInUserResolver currentUserResolver)
        {
            DbContext   = dbContext;
            CurrentUserResolver = currentUserResolver;
        }

        public Task<T> Get<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return Fetch(expression).SingleOrDefaultAsync();
        }

        public Task<TResult> Get<T, TResult>(Expression<Func<T, bool>>    expression,
                                            Expression<Func<T, TResult>> propertiesPickupExpression) where T : class
        {
            return Fetch(expression, propertiesPickupExpression).SingleOrDefaultAsync();
        }

        public IQueryable<T> Fetch<T>(Expression<Func<T, bool>> expression = null) where T : class
        {
            var query = DbContext.Set<T>().AsQueryable();

            if (expression != null)
            {
                query = query.Where(expression);
            }

            return query;
        }

        public IQueryable<TResult> Fetch<T, TResult>(Expression<Func<T, bool>>    expression,
                                                     Expression<Func<T, TResult>> propertiesPickupExpression)
            where T : class
        {
            if (propertiesPickupExpression == null)
                throw new ArgumentNullException(nameof(propertiesPickupExpression));

            var query = Fetch(expression);

            return query.Select(propertiesPickupExpression);
        }
        
        public Task<int> Count<T>(Expression<Func<T, bool>> expression = null) where T : class
        {
            if (expression == null)
                return DbContext.Set<T>().CountAsync();

            return DbContext.Set<T>().CountAsync(expression);
        }

        public async Task Insert<T>(T entity) where T : class
        {
            await DbContext.Set<T>().AddAsync(entity);
        }

        public Task Insert<T>(IEnumerable<T> entities) where T : class
        {
            throw new NotImplementedException();
        }

        public int CopyRecords<TSource, TTarget>(Expression<Func<TSource, bool>>    conditionExpression,
                                                 Expression<Func<TSource, TTarget>> copyExpression)
            where TSource : class where TTarget : class
        {
            throw new NotImplementedException();
        }

        public int Update<T>(Expression<Func<T, bool>> conditionExpression,
                             object                    updateExpression,
                             int?                      expectedAffectedRows = null) where T : class
        {
            throw new NotImplementedException();
        }

        public int Update<T>(Expression<Func<T, bool>> conditionExpression,
                             Expression<Func<T, T>>    updateExpression,
                             int?                      expectedAffectedRows = null) where T : class
        {
            throw new NotImplementedException();
        }

        public Task DeleteOne<T>(Expression<Func<T, bool>> conditionExpression, bool forceHardDelete = false)
            where T : class
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteMany<T>(Expression<Func<T, bool>> conditionExpression, bool forceHardDelete = false)
            where T : class
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> ExecuteQuery<T>(string sql, params SqlParameter[] parameters) where T : class
        {
            throw new NotImplementedException();
        }

        public TResult ExecuteScala<TResult>(string sql, params SqlParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public Task<T> RunInTransaction<T>(Func<T> action)
        {
            throw new NotImplementedException();
        }

        public Task<T> RunInTransaction<T>(Func<Task<T>> action) where T : struct
        {
            throw new NotImplementedException();
        }

        public Task RunInTransaction(Action action)
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}