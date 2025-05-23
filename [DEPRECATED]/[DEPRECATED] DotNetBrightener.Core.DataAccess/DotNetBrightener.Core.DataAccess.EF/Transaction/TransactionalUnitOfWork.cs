﻿using System;
using System.Transactions;
using DotNetBrightener.Core.DataAccess.Abstractions.Transaction;

namespace DotNetBrightener.Core.DataAccess.EF.Transaction
{
    internal class TransactionalUnitOfWork : ITransactionalUnitOfWork
    {
        private readonly TransactionScope _transactionScope;

        private bool _roolback = false;

        public TransactionalUnitOfWork()
        {
            _transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled);
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