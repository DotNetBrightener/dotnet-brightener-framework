using DotNetBrightener.Core.DataAccess.Abstractions.Transaction;
using System;
using System.Threading.Tasks;

namespace DotNetBrightener.Core.DataAccess.Transaction
{
    public class TransactionManager : ITransactionManager
    {
        public ITransactionalUnitOfWork BeginTransaction()
        {
            var transactionManager = new TransactionalUnitOfWork();
            return transactionManager;
        }


        public async Task<T> RunInTransaction<T>(Func<T> action) where T : struct
        {
            using var transaction = BeginTransaction();
            try
            {
                var result = action();

                return result;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task RunInTransaction(Action action)
        {
            using var transaction = BeginTransaction();
            try
            {
                action();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}