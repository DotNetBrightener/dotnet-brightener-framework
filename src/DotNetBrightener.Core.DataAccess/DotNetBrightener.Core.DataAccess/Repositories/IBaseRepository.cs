using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DotNetBrightener.Core.DataAccess.Repositories
{
    /// <summary>
    /// Represents shared Repository contracts, provides methods to perform database access operations
    /// </summary>
    public interface IBaseRepository : IDisposable
    {
        /// <summary>
        ///     Retrieves a specific record of type <typeparamref name="T"/> with the given <see cref="expression"/>
        /// </summary>
        /// <typeparam name="T">The type of the entity</typeparam>
        /// <param name="expression">
        ///     The expression describes how to pick the record
        /// </param>
        /// <returns>
        ///     The record of specified <see cref="T"/> if found, otherwise, <c>null</c>
        /// </returns>
        T Get<T>(Expression<Func<T, bool>> expression) where T : class;

        /// <summary>
        ///     Retrieves a specific record of type <typeparamref name="T"/> with the given <see cref="expression"/>, then map it to an instance of <typeparamref name="TResult"/>
        /// </summary>
        /// <typeparam name="T">The type of the entity</typeparam>
        /// <typeparam name="TResult">The type of the result</typeparam>
        /// <param name="expression">
        ///     The expression describes how to pick the record
        /// </param>
        /// <param name="propertiesPickupExpression">
        ///     The expression describes how to map the <typeparamref name="T"/> to <typeparamref name="TResult"/>
        /// </param>
        /// <returns>
        ///     The record of <typeparamref name="TResult"/> mapped from <typeparamref name="T"/> if found, otherwise, <c>null</c>
        /// </returns>
        TResult Get<T, TResult>(Expression<Func<T, bool>>    expression,
                                Expression<Func<T, TResult>> propertiesPickupExpression) where T : class;

        /// <summary>
        ///     Retrieves the records from the table associated with the entity type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="expression">The expression describes how to pick/filter the records</param>
        /// <returns>
        ///     A collection of the records of specified <see cref="T"/> that satisfy the <see cref="expression"/>
        /// </returns>
        IQueryable<T> Fetch<T>(Expression<Func<T, bool>> expression = null) where T : class;

        /// <summary>
        ///     Retrieves records of type <typeparamref name="T"/> with the given <see cref="expression"/>, then map it to instances of <typeparamref name="TResult"/>
        /// </summary>
        /// <typeparam name="T">The type of the entity</typeparam>
        /// <typeparam name="TResult">The type of the result</typeparam>
        /// <param name="expression">
        ///     The expression describes how to pick the record
        /// </param>
        /// <param name="propertiesPickupExpression">
        ///     The expression describes how to map the <typeparamref name="T"/> to <typeparamref name="TResult"/>
        /// </param>
        /// <returns>
        ///     A collection of <typeparamref name="TResult"/> records mapped from <typeparamref name="T"/>
        /// </returns>
        IQueryable<TResult> Fetch<T, TResult>(Expression<Func<T, bool>>    expression, Expression<Func<T, TResult>> propertiesPickupExpression) where T : class;

        /// <summary>
        ///     Retrieves the sorted records in ascending order from the table associated with the entity type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="expression">The expression describes how to pick/filter the records</param>
        /// <param name="orderExpression">The expression describes how to sort the records</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>
        ///     A collection of sorted records of <typeparamref name="T"/> that satisfy the <see cref="expression"/>
        /// </returns>
        IQueryable<T> OrderedFetch<T>(Expression<Func<T, bool>>   expression      = null,
                                      Expression<Func<T, object>> orderExpression = null,
                                      int?                        pageSize        = null,
                                      int?                        pageIndex       = null)
            where T : class;

        /// <summary>
        ///     Retrieves the sorted records in ascending order from the table associated with the entity type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="expression">The expression describes how to pick/filter the records</param>
        /// <param name="propertiesPickupExpression">
        ///     The expression describes how to map the <typeparamref name="T"/> to <typeparamref name="TResult"/>
        /// </param>
        /// <param name="orderExpression">The expression describes how to sort the records</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>
        ///     A collection of sorted records of <typeparamref name="T"/> that satisfy the <see cref="expression"/>
        /// </returns>
        IQueryable<TResult> OrderedFetch<T, TResult>(Expression<Func<T, bool>>    expression                 = null,
                                                     Expression<Func<T, TResult>> propertiesPickupExpression = null,
                                                     Expression<Func<T, object>>  orderExpression            = null,
                                                     int?                         pageSize                   = null,
                                                     int?                         pageIndex                  = null)
            where T : class;

        /// <summary>
        ///     Retrieves the sorted records in descending order from the table associated with the entity type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="expression">The expression describes how to pick/filter the records</param>
        /// <param name="orderExpression">The expression describes how to sort the records</param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>
        ///     A collection of sorted records of <typeparamref name="T"/> that satisfy the <see cref="expression"/>
        /// </returns>
        IQueryable<T> OrderedDescendingFetch<T>(Expression<Func<T, bool>>   expression      = null,
                                                Expression<Func<T, object>> orderExpression = null,
                                                int?                        pageSize        = null,
                                                int?                        pageIndex       = null)
            where T : class;

        /// <summary>
        ///     Retrieves the number of records of <typeparamref name="T"/> that satisfies the <see cref="expression"/>
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="expression">The expression describes how to pick/filter the records</param>
        /// <returns>
        /// The number of records that satisfies the <see cref="expression"/>
        /// </returns>
        int Count<T>(Expression<Func<T, bool>> expression = null) where T : class;

        /// <summary>
        ///     Insert a record of type <typeparamref name="T"/> into the database
        /// </summary>
        /// <typeparam name="T">The type of the record</typeparam>
        /// <param name="entity">The record to insert into the database</param>
        Task Insert<T>(T entity) where T : class;

        /// <summary>
        ///     Insert multiple records of type <typeparamref name="T"/> into the database
        /// </summary>
        /// <typeparam name="T">The type of the records</typeparam>
        /// <param name="entities">The records to insert into the database</param>
        Task Insert<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        ///     Copies the records that match the <see cref="conditionExpression"/> to the <typeparamref name="TTarget"/> table,
        ///     using the <see cref="copyExpression"/>
        /// </summary>
        /// <typeparam name="TSource">The type of source records</typeparam>
        /// <typeparam name="TTarget">The type of target records</typeparam>
        /// <param name="conditionExpression">
        ///     The expression describes how to filter the records
        /// </param>
        /// <param name="copyExpression">
        ///     The expression describes how to create or copy the records
        /// </param>
        /// <returns>
        ///     Number of rows copied
        /// </returns>
        int CopyRecords<TSource, TTarget>(Expression<Func<TSource, bool>>    conditionExpression,
                                          Expression<Func<TSource, TTarget>> copyExpression)
            where TSource : class
            where TTarget : class;

        ///  <summary>
        ///      Updates records of type <typeparamref name="T"/> from the query using an expression without retrieving entities
        ///  </summary>
        ///  <typeparam name="T">The entities' type of the query</typeparam>
        ///  <param name="conditionExpression">The query to update the entities from, without retrieving them</param>
        ///  <param name="updateExpression"></param>
        ///  <param name="expectedAffectedRows">
        ///      Expecting number of entities affected. <br />
        ///      If the actual result is different than the provided parameter, an exception will be thrown
        ///  </param>
        ///  <exception cref="NotFoundException">Thrown if no entity found for updating.</exception>
        ///  <exception cref="InvalidOperationException">Thrown if number of entities got updated differs from the provided expectation.</exception>
        int Update<T>(Expression<Func<T, bool>> conditionExpression,
                      object                    updateExpression,
                      int?                      expectedAffectedRows = null) where T : class;

        ///  <summary>
        ///      Updates records of type <typeparamref name="T"/> from the query using an expression without retrieving entities
        ///  </summary>
        ///  <typeparam name="T">The entities' type of the query</typeparam>
        ///  <param name="conditionExpression">The query to update the entities from, without retrieving them</param>
        ///  <param name="updateExpression"></param>
        ///  <param name="expectedAffectedRows">
        ///      Expecting number of entities affected. <br />
        ///      If the actual result is different than the provided parameter, an exception will be thrown
        ///  </param>
        ///  <exception cref="NotFoundException">Thrown if no entity found for updating.</exception>
        ///  <exception cref="InvalidOperationException">Thrown if number of entities got updated differs from the provided expectation.</exception>
        int Update<T>(Expression<Func<T, bool>> conditionExpression,
                      Expression<Func<T, T>>    updateExpression,
                      int?                      expectedAffectedRows = null) where T : class;


        /// <summary>
        ///     Delete a record of type <typeparamref name="T"/> from the query without retrieving the entity.
        /// </summary>
        /// <remarks>
        ///     If the <typeparamref name="T"/> can be soft-deleted, the entity will be marked as deleted
        /// </remarks>
        /// <param name="conditionExpression">The query to get the record to delete</param>
        /// <param name="forceHardDelete">Forces to hard-delete the entity, default is false which means soft-delete</param>
        /// <exception cref="NotFoundException">Thrown if no entity got deleted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if number of entities got deleted differs from the provided expectation.</exception>
        Task DeleteOne<T>(Expression<Func<T, bool>> conditionExpression, bool forceHardDelete = false) where T : class;

        /// <summary>
        ///     Delete multiple records of type <typeparamref name="T"/> from the query without retrieving them.
        /// </summary>
        /// <remarks>
        ///     If the <typeparamref name="T"/> can be soft-deleted, the entity will be marked as deleted
        /// </remarks>
        /// <param name="conditionExpression">The query to get the records to delete</param>
        /// <param name="forceHardDelete">Forces to hard-delete the entity, default is false which means soft-delete</param>
        /// <exception cref="NotFoundException">Thrown if no entity got deleted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if number of entities got deleted differs from the provided expectation.</exception>
        /// <returns></returns>
        Task<int> DeleteMany<T>(Expression<Func<T, bool>> conditionExpression, bool forceHardDelete = false) where T : class;

        /// <summary>
        /// Executes SQL query against a table associated with given entity type, and returns collection of the results.
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="sql">The SQL statement</param>
        /// <param name="parameters">The parameters to execute with the <see cref="sql"/></param>
        /// <returns>
        /// A collection of records of type <see cref="T"/>
        /// </returns>
        Task<IEnumerable<T>> ExecuteQuery<T>(string sql, params SqlParameter[] parameters) where T : class;

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query.
        /// Additional columns or rows are ignored.
        /// </summary>
        /// <typeparam name="TResult">The type of the result expected to get.</typeparam>
        /// <param name="sql">The SQL statement</param>
        /// <param name="parameters">The parameters to execute with the <see cref="sql"/></param>
        /// <returns>The result in type <see cref="TResult"/></returns>
        TResult ExecuteScala<TResult>(string sql, params SqlParameter[] parameters);

        /// <summary>
        /// Executes SQL statement, without querying
        /// </summary>
        /// <param name="sql">The SQL statement</param>
        /// <param name="parameters">The parameters to execute with the <see cref="sql"/></param>
        int ExecuteNonQuery(string sql, params SqlParameter[] parameters);

        /// <summary>
        /// Executes the given <see cref="action"/> in a transactional scope of work
        /// </summary>
        /// <typeparam name="T">Type of the returned result from the <see cref="action"/></typeparam>
        /// <param name="action">The action to be executed within the transaction</param>
        /// <returns>Result returned from the <see cref="action"/> if transaction is successfully finished</returns>
        Task<T> RunInTransaction<T>(Func<T> action);

        Task<T> RunInTransaction<T>(Func<Task<T>> action) where T : struct;

        /// <summary>
        /// Executes the given <see cref="action"/> in a transactional scope of work
        /// </summary>
        /// <param name="action">The action to be executed within the transaction</param>
        /// <returns></returns>
        Task RunInTransaction(Action action);
    }
}