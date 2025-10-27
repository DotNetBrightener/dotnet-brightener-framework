using Microsoft.EntityFrameworkCore.Storage;

namespace DotNetBrightener.DataAccess.EF.TransactionManagement;

/// <summary>
///     Manages database transactions at the scope level.
///     Provides centralized transaction management for all repository operations within a single request.
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// Gets a value indicating whether there is an active transaction.
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Gets the current database transaction, if any.
    /// </summary>
    IDbContextTransaction? CurrentTransaction { get; }

    /// <summary>
    /// Begins a new database transaction for the current request.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction and saves all changes to the database.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction, discarding all changes.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RollbackTransactionAsync();

    /// <summary>
    /// Ensures that a transaction is active. Throws an exception if no transaction is active.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no active transaction exists.</exception>
    void EnsureTransactionActive();
}
