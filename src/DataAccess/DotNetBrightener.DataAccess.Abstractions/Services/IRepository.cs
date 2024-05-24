using System.Linq.Expressions;

namespace DotNetBrightener.DataAccess.Services;

/// <summary>
///     Represents shared Repository contracts, provides methods to perform database access operations
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
    T? Get<T>(Expression<Func<T, bool>> expression)
        where T : class;

    /// <summary>
    ///     Retrieves first found record of type <typeparamref name="T"/> with the given <paramref name="expression"/>
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the entity
    /// </typeparam>
    /// <param name="expression">
    ///     The expression describes how to pick/filter the records
    /// </param>
    /// <returns>
    ///     The first found record of specified <see cref="T"/> if found, otherwise, <c>null</c>
    /// </returns>
    T? GetFirst<T>(Expression<Func<T, bool>> expression) where T : class;

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
    TResult? Get<T, TResult>(Expression<Func<T, bool>>?   expression,
                             Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class;

    /// <summary>
    ///     Retrieves the first found record of type <typeparamref name="T"/> with the given <paramref name="expression"/>
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
    ///     The first found record of specified <see cref="T"/> if found, otherwise, <c>null</c>
    /// </returns>
    TResult? GetFirst<T, TResult>(Expression<Func<T, bool>>?   expression,
                                  Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class;

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
    IQueryable<T> Fetch<T>(Expression<Func<T, bool>>? expression = null)
        where T : class;


    /// <summary>
    ///     Retrieves the history records for the given entity record,
    ///     from the table associated with the entity type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">
    ///     The type of entity
    /// </typeparam>
    /// <param name="expression">The expression describes how to pick/filter the records</param>
    /// <param name="from">
    ///     Specifies the lower boundary of the date range of the history records
    /// </param>
    /// <param name="to">
    ///     Specifies the upper boundary of the date range of the history records
    /// </param>
    /// <returns>
    ///     A collection of the history records of specified <see cref="T"/> that satisfy the <paramref name="expression"/>
    /// </returns>
    IQueryable<T> FetchHistory<T>(Expression<Func<T, bool>>? expression,
                                  DateTimeOffset?            from,
                                  DateTimeOffset?            to)
        where T : class, new();

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
    IQueryable<TResult> Fetch<T, TResult>(Expression<Func<T, bool>>?   expression,
                                          Expression<Func<T, TResult>> propertiesPickupExpression)
        where T : class;

    /// <summary>
    ///     Retrieves the number of records of <typeparamref name="T"/> that satisfies the <paramref name="expression"/>
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="expression">The expression describes how to pick/filter the records</param>
    /// <returns>
    /// The number of records that satisfies the <paramref name="expression"/>
    /// </returns>
    int Count<T>(Expression<Func<T, bool>>? expression = null)
        where T : class;

    /// <summary>
    ///     Insert a record of type <typeparamref name="T"/> into the database
    /// </summary>
    /// <typeparam name="T">The type of the record</typeparam>
    /// <param name="entity">The record to insert into the database</param>
    void Insert<T>(T entity)
        where T : class;

    /// <summary>
    ///     Insert multiple records of type <typeparamref name="T"/> into the database
    /// </summary>
    /// <remarks>
    ///     This method is not working in multi-tenant environment.
    ///     If you need to perform insert multiple records in multi-tenant environment,
    ///     consider looping through the records.
    ///     That will be slower in performance, but will guarantee the tenant for the records are captured properly.
    /// </remarks>
    /// <typeparam name="T">The type of the records</typeparam>
    /// <param name="entities">The records to insert into the database</param>
    void InsertMany<T>(IEnumerable<T> entities)
        where T : class;

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
    int CopyRecords<TSource, TTarget>(Expression<Func<TSource, bool>>?   conditionExpression,
                                      Expression<Func<TSource, TTarget>> copyExpression)
        where TSource : class
        where TTarget : class;

    /// <summary>
    ///     Mark the given entity in updated status
    /// </summary>
    /// <typeparam name="T">
    ///     Type of the entity
    /// </typeparam>
    /// <param name="entity">
    ///     The entity to update
    /// </param>
    void Update<T>(T entity) where T : class;

    /// <summary>
    ///     Updates multiple records of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">
    ///     Type of the entity
    /// </typeparam>
    /// <param name="entities">
    ///     The entities to update
    /// </param>
    void UpdateMany<T>(IEnumerable<T> entities) where T : class;

    /// <summary>
    ///     Updates records of type <typeparamref name="T"/> from the query using an object that describes the changes without retrieving entities
    /// </summary>
    /// <typeparam name="T">
    ///     The entities' type of the query
    /// </typeparam>
    /// <param name="conditionExpression">
    ///     The query to filter which entities to be updated with <seealso cref="updateExpression"/>, without retrieving them
    /// </param>
    /// <param name="updateExpression">
    ///     The object contains the properties and their values to use to update the entities
    /// </param>
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
    int Update<T>(Expression<Func<T, bool>>? conditionExpression,
                        object               updateExpression,
                        int?                 expectedAffectedRows = null)
        where T : class;

    ///  <summary>
    ///      Updates records of type <typeparamref name="T"/> from the query using an expression without retrieving records
    ///  </summary>
    ///  <typeparam name="T">The entities' type of the query</typeparam>
    ///  <param name="conditionExpression">
    ///      The query to filter which entities to be updated with <seealso cref="updateExpression"/>, without retrieving them
    ///  </param>
    ///  <param name="updateExpression">
    ///      The expression describes how to update the entities
    ///  </param>
    ///  <param name="expectedAffectedRows">
    ///      Expecting number of entities affected. <br />
    ///      If the actual result is different from the provided value, an exception will be thrown
    ///  </param>
    ///  <exception cref="NotFoundException">
    ///     Thrown if no entity found for updating.
    /// </exception>
    ///  <exception cref="ExpectedAffectedRecordMismatch">
    ///     Thrown if number of entities got updated differs from the provided expectation.
    /// </exception>
    int Update<T>(Expression<Func<T, bool>>?   conditionExpression,
                        Expression<Func<T, T>> updateExpression,
                        int?                   expectedAffectedRows = null)
        where T : class;


    /// <summary>
    ///     Deletes a record of type <typeparamref name="T"/> from the query, without retrieving the record.
    /// </summary>
    /// <remarks>
    ///     If the <typeparamref name="T"/> can be soft-deleted, the entity may be marked as deleted
    /// </remarks>
    /// <param name="conditionExpression">
    ///     The expression of how to identify which record to delete
    /// </param>
    /// <param name="reason"></param>
    /// <param name="forceHardDelete">
    ///     Enforcing hard-deletion on the entity, default is <c>false</c> for soft-deletion
    /// </param>
    /// <exception cref="NotFoundException">
    ///     Thrown if no entity got deleted.
    /// </exception>
    /// <exception cref="ExpectedAffectedRecordMismatch">
    ///     Thrown if number of entities got deleted differs from the provided expectation.
    /// </exception>
    void DeleteOne<T>(Expression<Func<T, bool>>? conditionExpression, string reason = null, bool forceHardDelete = false)
        where T : class;

    /// <summary>
    ///     Deletes multiple records of type <typeparamref name="T"/> from the query, without retrieving the entities.
    /// </summary>
    /// <remarks>
    ///     If the <typeparamref name="T"/> can be soft-deleted, the entity may be marked as deleted
    /// </remarks>
    /// <param name="conditionExpression">
    ///     The query to get the records to delete
    /// </param>
    /// <param name="reason"></param>
    /// <param name="forceHardDelete">
    ///     Enforcing hard-deletion on the records, default is <c>false</c> for soft-deletion
    /// </param>
    int DeleteMany<T>(Expression<Func<T, bool>>? conditionExpression,
                            string              reason          = null,
                            bool                 forceHardDelete = false)
        where T : class;

    /// <summary>
    ///     Restores one record of type <typeparamref name="T"/>,
    ///     specified by the given <see cref="conditionExpression"/>,
    ///     without retrieving the entity.
    /// </summary>
    /// <remarks>
    ///     Only if the <typeparamref name="T"/> can be soft-deleted, the entity will be marked as non-deleted
    /// </remarks>
    /// <param name="conditionExpression">
    ///     The query to get the record to restore
    /// </param>
    /// <exception cref="NotFoundException">
    ///     Thrown if no entity got restored.
    /// </exception>
    /// <exception cref="ExpectedAffectedRecordMismatch">
    ///     Thrown if more than one entity is restored.
    /// </exception>
    void RestoreOne<T>(Expression<Func<T, bool>>? conditionExpression) where T : class;

    /// <summary>
    ///     Restores multiple records of type <typeparamref name="T"/>,
    ///     specified by the given <see cref="conditionExpression"/>,
    ///     without retrieving the entities.
    /// </summary>
    /// <remarks>
    ///     Only if the <typeparamref name="T"/> can be soft-deleted, the entity will be marked as non-deleted
    /// </remarks>
    /// <param name="conditionExpression">
    ///     The query to get the records to restore
    /// </param>
    /// <returns>
    ///     Number of records restored from deletion
    /// </returns>
    int RestoreMany<T>(Expression<Func<T, bool>>? conditionExpression) where T : class;

    /// <summary>
    ///     Commits all changes into the database, if any
    /// </summary>
    int CommitChanges();
}