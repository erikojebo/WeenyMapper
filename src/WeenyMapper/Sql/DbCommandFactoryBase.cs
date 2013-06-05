using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using WeenyMapper.Extensions;

namespace WeenyMapper.Sql
{
    public abstract class DbCommandFactoryBase : IDbCommandFactory
    {
        private readonly string _connectionString;
        private readonly IList<ConnectionScope> _liveConnectionScopes = new List<ConnectionScope>();
        private readonly List<TransactionScope> _liveTransactionScopes = new List<TransactionScope>();

        protected DbCommandFactoryBase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool Matches(string connectionString)
        {
            return _connectionString.ToLower() == connectionString.ToLower();
        }

        public ConnectionScope BeginConnection()
        {
            var connection = CreateConnection();

            Open(connection);

            var connectionScope = new ConnectionScope(this, connection);

            _liveConnectionScopes.Add(connectionScope);

            return connectionScope;
        }

        private static void Open(DbConnection connection)
        {
            if (connection.State == ConnectionState.Closed)
                connection.Open();
        }

        public void EndConnection(ConnectionScope connectionScope)
        {
            _liveConnectionScopes.Remove(connectionScope);

            if (_liveConnectionScopes.IsEmpty())
                DisposeConnection(connectionScope);
        }

        private static void DisposeConnection(ConnectionScope connectionScope)
        {
            if (connectionScope.Connection.State == ConnectionState.Open)
                connectionScope.Connection.Close();

            connectionScope.Connection.Dispose();
        }

        public TransactionScope BeginTransaction()
        {
            var connectionScope = BeginConnection();
            var transaction = CreateTransaction(connectionScope);

            var transactionScope = new TransactionScope(this, transaction, connectionScope);

            _liveTransactionScopes.Add(transactionScope);

            return transactionScope;
        }

        private DbTransaction CreateTransaction(ConnectionScope connectionScope)
        {
            if (_liveTransactionScopes.Any())
                return _liveTransactionScopes.First().Transaction;

            return connectionScope.BeginTransaction();
        }

        public void EndTransaction(TransactionScope transactionScope)
        {
            _liveTransactionScopes.Remove(transactionScope);

            if (_liveTransactionScopes.IsEmpty())
            {
                EndConnection(transactionScope.ConnectionScope);
                DisposeTransaction(transactionScope);
            }
        }

        public DbCommand CreateCommand(string commandText)
        {
            var command = CreateNewCommand(commandText);

            if (_liveTransactionScopes.Any())
                command.Transaction = _liveTransactionScopes.First().Transaction;

            return command;
        }

        protected abstract DbCommand CreateNewCommand(string commandText);
        public abstract DbParameter CreateParameter(string name, object value);
        protected abstract DbConnection CreateNewConnection(string connectionString);

        public virtual DbConnection CreateConnection()
        {
            if (_liveConnectionScopes.Any())
                return _liveConnectionScopes.First().Connection;

            return CreateNewConnection(_connectionString);
        }

        public virtual DbParameter CreateParameter(CommandParameter commandParameter)
        {
            return CreateParameter(commandParameter.Name, commandParameter.Value);
        }

        public virtual DbCommand CreateCommand()
        {
            return CreateCommand(null);
        }

        public void Dispose()
        {
            foreach (var liveTransactionScope in _liveTransactionScopes)
            {
                DisposeTransaction(liveTransactionScope);
            }

            foreach (var liveConnectionScope in _liveConnectionScopes)
            {
                DisposeConnection(liveConnectionScope);
            }
        }

        private void DisposeTransaction(TransactionScope liveTransactionScope)
        {
            liveTransactionScope.Transaction.Dispose();
        }
    }
}