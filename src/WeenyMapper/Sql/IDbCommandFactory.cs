using System.Data.Common;

namespace WeenyMapper.Sql
{
    public interface IDbCommandFactory
    {
        DbCommand CreateCommand();
        DbCommand CreateCommand(string commandText);
        DbParameter CreateParameter(string name, object value);
        DbParameter CreateParameter(CommandParameter commandParameter);
        DbConnection CreateConnection(string connectionString);
        ConnectionScope BeginConnection(string connectionString);
        void EndConnection(ConnectionScope connectionScope);
        TransactionScope BeginTransaction(string connectionString);
        void EndTransaction(TransactionScope transactionScope);
    }
}