using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DotNetBrightener.Core.DataAccess.Abstractions.Repositories
{
    /// <summary>
    /// Represents shared Repository contracts, provides methods to perform database access operations
    /// </summary>
    public interface IRepository : IDisposable
    {
        /// <summary>
        ///     Retrieves a specific record of type <typeparamref name="T"/> with the given <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the entity
        /// </typeparam>
        /// <param name="expression">
        ///     The expression describes how to pick/filter the records
        /// </param>
        /// <returns>
        ///     The record of specified <see cref="T"/> if found, otherwise, <c>null</c>
        /// </returns>
        Task<T> Get<T>(Expression<Func<T, bool>> expression) where T : class;

        /// <summary>
        ///     Retrieves a specific record of type <typeparamref name="T"/> with the given <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the entity
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The type of the result object
        /// </typeparam>
        /// <param name="expression">
        ///     The expression describes how to pick/filter the records
        /// </param>
        /// <param name="propertiesPickupExpression">
        ///     The expression describes how to transfer the record to the result object
        /// </param>
        /// <returns>
        ///     The record of specified <see cref="T"/> if found, otherwise, <c>null</c>
        /// </returns>
        Task<TResult> Get<T, TResult>(Expression<Func<T, bool>>    expression,
                                      Expression<Func<T, TResult>> propertiesPickupExpression) where T : class;

        /// <summary>
        ///     Retrieves the records from the table associated with the entity type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">
        ///     The type of entity
        /// </typeparam>
        /// <param name="expression">The expression describes how to pick/filter the records</param>
        /// <returns>
        ///     A collection of the records of specified <see cref="T"/> that satisfy the <paramref name="expression"/>
        /// </returns>
        IQueryable<T> Fetch<T>(Expression<Func<T, bool>> expression = null) where T : class;

        /// <summary>
        ///     Retrieves the records from the table associated with the entity type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">
        ///     The type of entity
        /// </typeparam>
        /// <typeparam name="TResult">
        ///     The type of the result object
        /// </typeparam>
        /// <param name="expression">The expression describes how to pick/filter the records</param> 
        /// <param name="propertiesPickupExpression">
        ///     The expression describes how to transfer the record to the result object
        /// </param>
        /// <returns>
        ///     A collection of the records of specified <see cref="T"/> that satisfy the <paramref name="expression"/>
        /// </returns>
        IQueryable<TResult> Fetch<T, TResult>(Expression<Func<T, bool>>    expression,
                                              Expression<Func<T, TResult>> propertiesPickupExpression) where T : class;

        /// <summary>
        ///     Retrieves the number of records of <typeparamref name="T"/> that satisfies the <paramref name="expression"/>
        /// </summary>
        /// <typeparam name="T">The type of entity</typeparam>
        /// <param name="expression">The expression describes how to pick/filter the records</param>
        /// <returns>
        /// The number of records that satisfies the <paramref name="expression"/>
        /// </returns>
        Task<int> Count<T>(Expression<Func<T, bool>> expression = null) where T : class;

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
        ///     Copies the records that match the <paramref name="conditionExpression"/> to the <typeparamref name="TTarget"/>
        ///     table, using the <paramref name="copyExpression"/>
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
        int CopyRecords<TSource, TTarget>(Expression<Func<TSource, bool>> conditionExpression,
                                          Expression<Func<TSource, TTarget>> copyExpression)
            where TSource : class
            where TTarget : class;

        /// <summary>
        ///     Updates records of type <typeparamref name="T"/> from the query using an object that describes the changes without retrieving entities
        /// </summary>
        /// <typeparam name="T">
        ///     The entities' type of the query
        /// </typeparam>
        /// <param name="conditionExpression">
        ///     The query to update the entities from, without retrieving them
        /// </param>
        /// <param name="updateExpression"></param>
        /// <param name="expectedAffectedRows">
        ///     Expecting number of entities affected. <br />
        ///     If the actual result is different than the provided parameter, an exception will be thrown
        /// </param>
        /// <exception cref="NotFoundException">
        ///    Thrown if no entity found for updating.
        /// </exception>
        ///  <exception cref="ExpectedAffectedRecordMismatch">
        ///     Thrown if number of entities got updated differs from the provided expectation.
        /// </exception>
        int Update<T>(Expression<Func<T, bool>> conditionExpression,
                      object updateExpression,
                      int? expectedAffectedRows = null) where T : class;

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
        ///  <exception cref="ExpectedAffectedRecordMismatch">Thrown if number of entities got updated differs from the provided expectation.</exception>
        int Update<T>(Expression<Func<T, bool>> conditionExpression,
                      Expression<Func<T, T>> updateExpression,
                      int? expectedAffectedRows = null) where T : class;


        /// <summary>
        ///     Deletes a record of type <typeparamref name="T"/> from the query, without retrieving the entity.
        /// </summary>
        /// <remarks>
        ///     If the <typeparamref name="T"/> can be soft-deleted, the entity may be marked as deleted
        /// </remarks>
        /// <param name="conditionExpression">
        ///     The query to get the record to delete
        /// </param>
        /// <param name="forceHardDelete">
        ///     Enforcing hard-deletion on the entity, default is <c>false</c> for soft-deletion
        /// </param>
        /// <exception cref="NotFoundException">
        ///     Thrown if no entity got deleted.
        /// </exception>
        /// <exception cref="ExpectedAffectedRecordMismatch">
        ///     Thrown if number of entities got deleted differs from the provided expectation.
        /// </exception>
        Task DeleteOne<T>(Expression<Func<T, bool>> conditionExpression, bool forceHardDelete = false) where T : class;

        /// <summary>
        ///     Deletes multiple records of type <typeparamref name="T"/> from the query, without retrieving the entities.
        /// </summary>
        /// <remarks>
        ///     If the <typeparamref name="T"/> can be soft-deleted, the entity may be marked as deleted
        /// </remarks>
        /// <param name="conditionExpression">
        ///     The query to get the records to delete
        /// </param>
        /// <param name="forceHardDelete">
        ///     Enforcing hard-deletion on the records, default is <c>false</c> for soft-deletion
        /// </param>
        Task<int> DeleteMany<T>(Expression<Func<T, bool>> conditionExpression, bool forceHardDelete = false) where T : class;
    }
}