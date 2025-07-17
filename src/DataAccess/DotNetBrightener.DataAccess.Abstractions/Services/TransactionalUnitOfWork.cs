using System.Transactions;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.Services;

internal class TransactionalUnitOfWork : ITransactionalUnitOfWork
{
    private readonly TransactionScope _transactionScope;
    private readonly ILogger          _logger;

    private                 bool _needRollingBack = false;
    private static readonly Lock _lock            = new();

    public TransactionalUnitOfWork(ILogger<TransactionalUnitOfWork> logger)
    {
        _logger = logger;
        _transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew,
                                                 TransactionScopeAsyncFlowOption.Enabled);
    }

    public void Rollback()
    {
        _logger.LogInformation("Marking the transaction to roll back...");
        _needRollingBack = true;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            try
            {
                if (!_needRollingBack)
                {
                    _logger.LogInformation("No roll back needed. Submitting the transaction...");
                    // without calling this, all transactions will be rolled-back
                    _transactionScope.Complete();
                }
                else
                {
                    _logger.LogInformation("Rolling back the transaction...");
                }

                _transactionScope.Dispose();
            }
            catch (ObjectDisposedException exception)
            {
                // transaction already disposed, keeping this for debugging purpose
                _logger.LogWarning(exception, "Got ObjectDisposedException while cleaning up.");
            }
            finally
            {
                _needRollingBack = false;
            }
        }
    }
}