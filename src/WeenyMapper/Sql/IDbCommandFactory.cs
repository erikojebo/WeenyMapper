using System;
using System.Data.Common;

namespace WeenyMapper.Sql
{
    public interface IDbCommandFactory : IDisposable
    {
        DbCommand CreateCommand();
        DbCommand CreateCommand(string commandText);
        DbParameter CreateParameter(string name, object value);
        DbParameter CreateParameter(CommandParameter commandParameter);
        DbConnection CreateConnection();
        ConnectionScope BeginConnection();
        void EndConnection(ConnectionScope connectionScope);
        TransactionScope BeginTransaction();
        void EndTransaction(TransactionScope transactionScope);
        bool Matches(string connectionString);
    }
}