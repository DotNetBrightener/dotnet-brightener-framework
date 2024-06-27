using System.Linq.Expressions;

namespace DotNetBrightener.DataAccess.Services;

public interface IBaseDataService<TEntity>: IDisposable
{
    /// <summary>
    ///     Returns a specific record that matches the given expression
    /// </summary>
    /// <param name="expression">The condition to retrieve the record</param>
    /// <returns>The entity record, if matched, otherwise, <c>null</c></returns>
    TEntity? Get(Expression<Func<TEntity, bool>> expression);

    /// <summary>
    ///     Returns a specific record that matches the given expression
    /// </summary>
    /// <param name="expression">The condition to retrieve the record</param>
    /// <returns>The entity record, if matched, otherwise, <c>null</c></returns>
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> expression);

    /// <summary>
    ///     Generates a fetch query to the given history table of the entity, optionally provides the condition for filtering
    /// </summary>
    /// <param name="expression">The condition for filtering records in the query</param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns>An IQueryable of the history records of the requested entity</returns>
    IQueryable<TEntity> FetchHistory(Expression<Func<TEntity, bool>>? expression = null,
                                     DateTimeOffset?                  from       = null,
                                     DateTimeOffset?                  to         = null);

    /// <summary>
    ///     Generates a fetch query to the given entity table, optionally provides the condition for filtering
    /// </summary>
    /// <param name="expression">The condition for filtering records</param>
    /// <returns>An IQueryable of the collection of the requested entities</returns>
    IQueryable<TEntity> Fetch(Expression<Func<TEntity, bool>>? expression = null);

    /// <summary>
    ///     Generates a fetch query to the given entity table, and map it to a DTO using <see cref="propertiesPickupExpression"/>
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the entity
    /// </typeparam>
    /// <typeparam name="TResult">
    ///     The type of the DTO
    /// </typeparam>
    /// <param name="expression">
    ///     The condition for filtering records
    /// </param>
    /// <param name="propertiesPickupExpression">
    ///     The expression describes how to map the entity to the DTO
    /// </param>
    /// <returns>
    ///     An IQueryable of the collection of the mapped DTO records from the requested entities
    /// </returns>
    IQueryable<TResult> Fetch<TResult>(Expression<Func<TEntity, bool>>?   expression,
                                       Expression<Func<TEntity, TResult>> propertiesPickupExpression);


    /// <summary>
    ///     Retrieves the number of records of <typeparamref name="T"/> that satisfies the <paramref name="expression"/>
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="expression">The expression describes how to pick/filter the records</param>
    /// <returns>
    /// The number of records that satisfies the <paramref name="expression"/>
    /// </returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? expression = null);

    /// <summary>
    ///     Retrieves the number of records of <typeparamref name="T"/> that satisfies the <paramref name="expression"/>
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="expression">The expression describes how to pick/filter the records</param>
    /// <returns>
    /// The number of records that satisfies the <paramref name="expression"/>
    /// </returns>
    Task<int> CountNonDeletedAsync(Expression<Func<TEntity, bool>>? expression = null);


    /// <summary>
    ///     Generates a fetch query to the given entity table with the deleted records, optionally provides the condition for filtering
    /// </summary>
    /// <param name="expression">The condition for filtering records in the query</param>
    /// <returns>An IQueryable of the collection of the requested entities, which are deleted</returns>
    IQueryable<TEntity> FetchDeletedRecords(Expression<Func<TEntity, bool>>? expression = null);


    /// <summary>
    ///     Generates a fetch query to the given entity table with the non-deleted records, optionally provides the condition for filtering
    /// </summary>
    /// <remarks>
    ///     This method is deprecated. Use <see cref="FetchNonDeleted"/> instead.
    /// </remarks>
    /// <param name="expression">The condition for filtering records in the query</param>
    /// <returns>An IQueryable of the collection of the requested entities, which are not deleted</returns>
    [Obsolete("Use FetchNonDeleted() method instead")]
    IQueryable<TEntity> FetchActive(Expression<Func<TEntity, bool>>? expression = null);


    /// <summary>
    ///     Generates a fetch query to the given entity table with the non-deleted records, optionally provides the condition for filtering
    /// </summary>
    /// <param name="expression">The condition for filtering records in the query</param>
    /// <returns>An IQueryable of the collection of the requested entities, which are not deleted</returns>
    IQueryable<TEntity> FetchNonDeleted(Expression<Func<TEntity, bool>>? expression = null);

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
    ///     Inserts multiple records of the entity to the database
    /// </summary>
    /// <param name="entities">The records to insert</param>
    void InsertMany(IEnumerable<TEntity> entities);

    /// <summary>
    ///     Inserts multiple records of the entity to the database
    /// </summary>
    /// <param name="entities">The records to insert</param>
    Task InsertManyAsync(IEnumerable<TEntity> entities);

    /// <summary>
    ///     Inserts multiple records of the entity to the database
    /// </summary>
    /// <param name="entities">The records to insert</param>
    void BulkInsert(IEnumerable<TEntity> entities);

    /// <summary>
    ///     Inserts multiple records of the entity to the database
    /// </summary>
    /// <param name="entities">The records to insert</param>
    Task BulkInsertAsync(IEnumerable<TEntity> entities);

    /// <summary>
    ///     Updates a record of the entity to the database
    /// </summary>
    /// <param name="entity">The record to update</param>
    void Update(TEntity entity);

    /// <summary>
    ///     Updates a record of the entity to the database
    /// </summary>
    /// <param name="entity">The record to update</param>
    Task UpdateAsync(TEntity entity);

    /// <summary>
    ///     Updates the given entity using the provided DTO
    /// </summary>
    /// <param name="entity">
    ///     The record to update
    /// </param>
    /// <param name="dto">
    ///     The data-transfer-object to update the entity with
    /// </param>
    /// <param name="propertiesToIgnoreUpdate">
    ///     An array of property names to ignore while updating the entity
    /// </param>
    void Update(TEntity entity, object dto, params string[] propertiesToIgnoreUpdate);

    /// <summary>
    ///     Updates the given entity using the provided DTO
    /// </summary>
    /// <param name="entity">
    ///     The record to update
    /// </param>
    /// <param name="dto">
    ///     The data-transfer-object to update the entity with
    /// </param>
    /// <param name="propertiesToIgnoreUpdate">
    ///     An array of property names to ignore while updating the entity
    /// </param>
    Task UpdateAsync(TEntity entity, object dto, params string[] propertiesToIgnoreUpdate);
    
    /// <summary>
    ///     Updates multiple records of the entity to the database
    /// </summary>
    /// <param name="entities">The records to update</param>
    void UpdateMany(params TEntity[] entities);

    /// <summary>
    ///     Update the matched record with the given filter expression, expected only one record affected
    /// </summary>
    /// <param name="filterExpression">The expression for selecting the record to update</param>
    /// <param name="updateExpression">The expression that describes the update instruction</param>
    Task UpdateOne(Expression<Func<TEntity, bool>>?   filterExpression,
                   Expression<Func<TEntity, TEntity>> updateExpression);

    /// <summary>
    ///     Update multiple matched records with the given filter expression
    /// </summary>
    /// <param name="filterExpression">The expression for selecting the records to update</param>
    /// <param name="updateExpression">The expression that describes the update instruction</param>
    Task<int> UpdateMany(Expression<Func<TEntity, bool>>?   filterExpression,
                         Expression<Func<TEntity, TEntity>> updateExpression);

    /// <summary>
    ///     Delete the matched record with the given filter expression, expected only one record affected
    /// </summary>
    /// <param name="filterExpression">
    ///     The expression describes how to get the record to delete
    /// </param>
    /// <param name="reason"></param>
    /// <param name="forceHardDelete">
    ///     Indicates whether the deletion is permanent. Default is <c>False</c> which marks the record as deleted
    /// </param>
    Task DeleteOne(Expression<Func<TEntity, bool>>? filterExpression, string reason = null, bool forceHardDelete = false);

    /// <summary>
    ///     Restore the matched deleted record with the given filter expression, expected only one record affected
    /// </summary>
    /// <param name="filterExpression">
    ///     The expression for selecting the record to restore
    /// </param>
    Task RestoreOne(Expression<Func<TEntity, bool>>? filterExpression);


    /// <summary>
    ///     Delete multiple matched records with the given filter expression
    /// </summary>
    /// <param name="filterExpression">
    ///     The expression describes how to get the records to delete
    /// </param>
    /// <param name="reason"></param>
    /// <param name="forceHardDelete">
    ///     Indicates whether the deletion is permanent. Default is <c>False</c> which marks the records as deleted
    /// </param>
    Task<int> DeleteMany(Expression<Func<TEntity, bool>>? filterExpression,
                         string                           reason          = null,
                         bool                             forceHardDelete = false);

    /// <summary>
    ///     Restore the matched deleted records with the given filter expression
    /// </summary>
    /// <param name="filterExpression">
    ///     The expression describes how to get the records to restore
    /// </param>
    Task<int> RestoreMany(Expression<Func<TEntity, bool>>? filterExpression);
}