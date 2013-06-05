using System;
using System.Data.Common;
using WeenyMapper.Sql;

namespace WeenyMapper
{
    public class TransactionScope : IDisposable
    {
        private readonly IDbCommandFactory _commandFactory;

        public readonly DbTransaction Transaction;
        public readonly ConnectionScope ConnectionScope;

        public TransactionScope(IDbCommandFactory commandFactory, DbTransaction transaction, ConnectionScope connectionScope)
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
        }

        public virtual void CommitTransaction()
        {
            Transaction.Commit();
        }

        public virtual void Rollback()
        {
            Transaction.Rollback();
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