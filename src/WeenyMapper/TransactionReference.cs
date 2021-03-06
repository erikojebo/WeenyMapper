using System;
using System.Data;
using System.Data.Common;

namespace WeenyMapper
{
    public class TransactionReference : IDisposable
    {
        public readonly DbTransaction Transaction;
        private bool _isRolledBack;

        public TransactionReference(DbTransaction transaction)
        {
            Transaction = transaction;
        }

        public bool HasOpenConnection
        {
            get { return Transaction.Connection != null && Transaction.Connection.State == ConnectionState.Open; }
        }

        public void Rollback()
        {
            Transaction.Rollback();
            _isRolledBack = true;
        }

        public void WeakRollback()
        {
            if (!_isRolledBack)
                Rollback();
        }
        
        public void Commit()
        {
            Transaction.Commit();
        }

        public void Dispose()
        {
            Transaction.Dispose();
        }
    }
}