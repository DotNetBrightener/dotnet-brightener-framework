using System;
using System.Threading.Tasks;

namespace DotNetBrightener.Core.DataAccess.Abstractions.Transaction
{
    public interface ITransactionManager
    {
        /// <summary>
        /// Starts a new transactional scope of work
        /// </summary>
        /// <returns></returns>
        ITransactionalUnitOfWork BeginTransaction();

        Task<T> RunInTransaction<T>(Func<T> action) where T : struct;

        Task RunInTransaction(Action action);
    }
}