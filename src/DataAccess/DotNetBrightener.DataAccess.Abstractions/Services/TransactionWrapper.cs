using Microsoft.Extensions.Logging;

namespace DotNetBrightener.DataAccess.Services;

public class TransactionWrapper : ITransactionWrapper
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger          _logger;

    public TransactionWrapper(IServiceProvider            serviceProvider,
                              ILogger<TransactionWrapper> logger)
    {
        _serviceProvider = serviceProvider;
        _logger          = logger;
    }

    public ITransactionalUnitOfWork BeginTransaction()
    {
        var transactionManager = _serviceProvider.TryGet<TransactionalUnitOfWork>();

        return transactionManager;
    }


    public async Task<T> ExecuteWithTransaction<T>(Func<T> action) where T : struct
    {
        _logger.LogInformation("Initiating new transaction...");
        using var transaction = BeginTransaction();

        try
        {
            _logger.LogInformation("Executing the task...");

            var result = action();

            _logger.LogInformation("Task executed successfully...");
            return result;
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error occured while executing the task. Rolling back the changes...");
            transaction.Rollback();

            throw;
        }
    }

    public async Task ExecuteWithTransaction(Action action)
    {
        _logger.LogInformation("Initiating new transaction...");
        using var transaction = BeginTransaction();

        try
        {
            _logger.LogInformation("Executing the task...");

            action();

            _logger.LogInformation("Task executed successfully...");
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error occured while executing the task. Rolling back the changes...");
            transaction.Rollback();

            throw;
        }
    }
}