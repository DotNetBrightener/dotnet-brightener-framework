#nullable enable
using System.Linq.Expressions;

namespace DotNetBrightener.DataAccess.Services;

public interface IReadOnlyRepository : IDisposable
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
    Task<T?> GetAsync<T>(Expression<Func<T, bool>> expression)
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
    Task<T?> GetFirstAsync<T>(Expression<Func<T, bool>> expression) where T : class;

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
    ///     Retrieves the number of records of <typeparamref name="T"/> that satisfies the <paramref name="expression"/>
    /// </summary>
    /// <typeparam name="T">
    ///     The type of entity
    /// </typeparam>
    /// <param name="expression">
    ///     The expression describes how to pick/filter the records
    /// </param>
    /// <returns>
    ///     The number of records that satisfies the <paramref name="expression"/>
    /// </returns>
    Task<int> CountAsync<T>(Expression<Func<T, bool>>? expression = null)
        where T : class;

    /// <summary>
    ///     Retrieves the number of non-deleted records of <typeparamref name="T"/> that satisfies the <paramref name="expression"/>
    /// </summary>
    /// <typeparam name="T">
    ///     The type of entity
    /// </typeparam>
    /// <param name="expression">
    ///     The expression describes how to pick/filter the records
    /// </param>
    /// <returns>
    ///     The number of records that satisfies the <paramref name="expression"/>
    /// </returns>
    Task<int> CountNonDeletedAsync<T>(Expression<Func<T, bool>>? expression = null)
        where T : class;

    /// <summary>
    ///     Check if there is any record of <typeparamref name="T"/>, and if that satisfies the <paramref name="expression"/> if specified
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="expression">
    ///     The expression describes how to pick/filter the records
    /// </param>
    /// <returns>
    ///     <c>true</c> if there is any record satisfies the <paramref name="expression"/>. Otherwise, <c>false</c>
    /// </returns>
    bool Any<T>(Expression<Func<T, bool>>? expression = null)
        where T : class;

    /// <summary>
    ///     Check if there is any record of <typeparamref name="T"/>, and if that satisfies the <paramref name="expression"/> if specified
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="expression">
    ///     The expression describes how to pick/filter the records
    /// </param>
    /// <returns>
    ///     <c>true</c> if there is any record satisfies the <paramref name="expression"/>. Otherwise, <c>false</c>
    /// </returns>
    Task<bool> AnyAsync<T>(Expression<Func<T, bool>>? expression = null)
        where T : class;
}