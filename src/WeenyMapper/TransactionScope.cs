using System;
using System.Data.Common;
using WeenyMapper.Sql;

namespace WeenyMapper
{
    public class TransactionScope : IDisposable
    {
        private readonly IDbCommandFactory _commandFactory;
        private bool _isTransactionDisposed;

        public readonly TransactionReference Transaction;
        public readonly ConnectionScope ConnectionScope;
        private bool _hasCompleted;

        public TransactionScope(IDbCommandFactory commandFactory, TransactionReference transaction, ConnectionScope connectionScope)
        {
            _commandFactory = commandFactory;
            Transaction = transaction;
            ConnectionScope = connectionScope;
        }

        protected TransactionScope()
        {
            
        }

        public virtual void Dispose()
        {
            _commandFactory.EndTransaction(this);

            if (!_hasCompleted)
                Transaction.WeakRollback();
        }

        internal void DisposeTransaction()
        {
            if (Transaction != null && !_isTransactionDisposed)
            {
                Transaction.Dispose();
                _isTransactionDisposed = true;
            }
        }

        public virtual void Commit()
        {
            Transaction.Commit();
            _hasCompleted = true;
        }

        public virtual void Rollback()
        {
            Transaction.Rollback();
            _hasCompleted = true;
        }

        public virtual bool Matches(TransactionScope transactionScope)
        {
            return transactionScope.Transaction == Transaction;
        }

        public virtual bool Matches(ConnectionScope connectionScope)
        {
            return ConnectionScope.Matches(connectionScope);
        }
    }
}