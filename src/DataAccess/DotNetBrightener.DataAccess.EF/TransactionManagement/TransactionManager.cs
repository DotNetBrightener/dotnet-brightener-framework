#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.EF.TransactionManagement;

/// <summary>
///     Manages database transactions at the HTTP request scope level.
///     This class is registered as a scoped service, ensuring one instance per HTTP request.
/// </summary>
/// <typeparam name="TDbContext">
///     The type of DbContext to manage transactions for.
/// </typeparam>
public class TransactionManager<TDbContext>(
    TDbContext                              dbContext,
    ILogger<TransactionManager<TDbContext>> logger)
    : ITransactionManager, IAsyncDisposable
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    private readonly ILogger<TransactionManager<TDbContext>> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private bool _disposed;

    /// <inheritdoc />
    public bool HasActiveTransaction => CurrentTransaction != null && !_disposed;

    /// <inheritdoc />
    public IDbContextTransaction? CurrentTransaction { get; private set; }

    /// <inheritdoc />
    public async Task BeginTransactionAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TransactionManager<TDbContext>));
        }

        if (HasActiveTransaction)
        {
            _logger.LogWarning("Attempted to begin a transaction when one is already active. Ignoring request.");

            return;
        }

        try
        {
            _logger.LogDebug("Beginning database transaction for request");
            CurrentTransaction = await _dbContext.Database.BeginTransactionAsync();
            _logger.LogInformation("Database transaction started successfully. Transaction ID: {TransactionId}",
                                   CurrentTransaction.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to begin database transaction");

            throw;
        }
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TransactionManager<TDbContext>));
        }

        if (!HasActiveTransaction)
        {
            _logger.LogWarning("Attempted to commit transaction when no active transaction exists");

            return;
        }

        try
        {
            _logger.LogDebug("Saving changes to database");

            if (_dbContext.ChangeTracker.HasChanges())
            {
                await _dbContext.SaveChangesAsync();

                _logger.LogDebug("Committing database transaction. Transaction ID: {TransactionId}",
                                 CurrentTransaction!.TransactionId);
                await CurrentTransaction.CommitAsync();

                _logger.LogInformation("Database transaction committed successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                             "Failed to commit database transaction. Transaction ID: {TransactionId}",
                             CurrentTransaction?.TransactionId);

            // Attempt to rollback on commit failure
            await RollbackTransactionAsync();

            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync()
    {
        if (_disposed)
        {
            _logger.LogDebug("Transaction manager already disposed, skipping rollback");

            return;
        }

        if (!HasActiveTransaction)
        {
            _logger.LogDebug("No active transaction to rollback");

            return;
        }

        try
        {
            _logger.LogWarning("Rolling back database transaction. Transaction ID: {TransactionId}",
                               CurrentTransaction!.TransactionId);
            await CurrentTransaction.RollbackAsync();
            _logger.LogInformation("Database transaction rolled back successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                             "Failed to rollback database transaction. Transaction ID: {TransactionId}",
                             CurrentTransaction?.TransactionId);
            // Don't rethrow rollback exceptions to avoid masking the original exception
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <inheritdoc />
    public void EnsureTransactionActive()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TransactionManager<TDbContext>));
        }

        if (!HasActiveTransaction)
        {
            throw new InvalidOperationException(
                                                "This operation requires an active database transaction. " +
                                                "Ensure that the TransactionMiddleware is properly configured and that this operation " +
                                                "is being called within an HTTP request context.");
        }
    }

    /// <summary>
    /// 	Disposes the current transaction without committing or rolling back.
    /// 	Used internally after commit or rollback operations.
    /// </summary>
    private async Task DisposeTransactionAsync()
    {
        if (CurrentTransaction != null)
        {
            await CurrentTransaction.DisposeAsync();
            CurrentTransaction = null;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            // If there's still an active transaction when disposing, it means
            // the request ended without explicit commit/rollback, so we rollback
            if (HasActiveTransaction)
            {
                _logger.LogWarning("Request ended with active transaction. Performing automatic rollback.");
                await RollbackTransactionAsync();
            }
        }
        finally
        {
            _disposed = true;
        }
    }
}
