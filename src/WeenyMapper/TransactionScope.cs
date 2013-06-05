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

        public void Dispose()
        {
            Transaction.Dispose();
            _commandFactory.EndTransaction(this);
        }

        public void Commit()
        {
            Transaction.Commit();
        }

        public void Rollback()
        {
            Transaction.Rollback();
        }

        public bool Matches(TransactionScope transactionScope)
        {
            return transactionScope.Transaction == Transaction;
        }

        public bool Matches(ConnectionScope connectionScope)
        {
            return ConnectionScope.Matches(connectionScope);
        }
    }
}