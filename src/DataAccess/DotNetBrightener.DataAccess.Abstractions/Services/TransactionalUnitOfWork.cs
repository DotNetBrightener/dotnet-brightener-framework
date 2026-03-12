using System.Transactions;
using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.Services;

internal class TransactionalUnitOfWork(ILogger<TransactionalUnitOfWork> logger) : ITransactionalUnitOfWork
{
    private readonly TransactionScope _transactionScope = new(TransactionScopeOption.RequiresNew,
                                                              TransactionScopeAsyncFlowOption.Enabled);

    private readonly ILogger _logger = logger;

    private                 bool _needRollingBack = false;
    private static readonly Lock Lock             = new();

    public void Rollback()
    {
        _logger.LogInformation("Marking the transaction to roll back...");
        _needRollingBack = true;
    }

    public void Dispose()
    {
        lock (Lock)
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