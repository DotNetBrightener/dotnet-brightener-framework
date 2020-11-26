using System;
using System.Transactions;

namespace DotNetBrightener.Integration.GraphQL.Transactions
{
    /// <summary>
    /// Represents a transactional unit-of-work, to support database rolling back
    /// </summary>
    public interface ITransactionalUnitOfWork : IDisposable
    {
        /// <summary>
        /// Marks the transaction should be rolled back due to some exceptions or intentionally
        /// </summary>
        void Rollback();
    }

    internal class TransactionalUnitOfWork : ITransactionalUnitOfWork
    {
        private readonly TransactionScope _transactionScope;

        private bool _roolback = false;

        public TransactionalUnitOfWork()
        {
            _transactionScope = new TransactionScope();
        }

        public void Rollback()
        {
            _roolback = true;
        }

        public void Dispose()
        {
            lock (this)
            {
                try
                {
                    if (!_roolback)
                    {
                        // without calling this, all transactions will be rolled-back
                        _transactionScope.Complete();
                    }

                    _transactionScope.Dispose();
                }
                catch (ObjectDisposedException exception)
                {
                    // transaction already disposed, keeping this for debugging purpose
                }
                finally
                {
                    _roolback = false;
                }
            }
        }
    }
}