using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.Services;

public class TransactionWrapper(
    IServiceProvider            serviceProvider,
    ILogger<TransactionWrapper> logger)
    : ITransactionWrapper
{
    public ITransactionalUnitOfWork BeginTransaction()
    {
        var transactionManager = serviceProvider.TryGet<TransactionalUnitOfWork>();

        return transactionManager;
    }


    public async Task<T> ExecuteWithTransaction<T>(Func<T> action) where T : struct
    {
        logger.LogInformation("Initiating new transaction...");
        using var transaction = BeginTransaction();

        try
        {
            logger.LogInformation("Executing the task...");

            var result = action();

            logger.LogInformation("Task executed successfully...");

            return result;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error occured while executing the task. Rolling back the changes...");
            transaction.Rollback();

            throw;
        }
    }

    public async Task ExecuteWithTransaction(Action action)
    {
        logger.LogInformation("Initiating new transaction...");
        using var transaction = BeginTransaction();

        try
        {
            logger.LogInformation("Executing the task...");

            action();

            logger.LogInformation("Task executed successfully...");
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Error occured while executing the task. Rolling back the changes...");
            transaction.Rollback();

            throw;
        }
    }
}