using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using WeenyMapper.Extensions;

namespace WeenyMapper.Sql
{
    public abstract class DbCommandFactoryBase : IDbCommandFactory
    {
        private readonly IList<ConnectionScope> _liveConnectionScopes = new List<ConnectionScope>();
        private readonly List<TransactionScope> _liveTransactionScopes = new List<TransactionScope>();

        public ConnectionScope BeginConnection(string connectionString)
        {
            var connection = CreateConnection(connectionString);

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

            if (ConnectionScopesMatching(connectionScope).IsEmpty())
            {
                Close(connectionScope);

                connectionScope.Connection.Dispose();
            }
        }

        private static void Close(ConnectionScope connectionScope)
        {
            if (connectionScope.Connection.State == ConnectionState.Open)
                connectionScope.Connection.Close();
        }

        public TransactionScope BeginTransaction(string connectionString)
        {
            var connectionScope = BeginConnection(connectionString);
            var transaction = CreateTransaction(connectionScope);

            var transactionScope = new TransactionScope(this, transaction, connectionScope);

            _liveTransactionScopes.Add(transactionScope);

            return transactionScope;
        }

        private DbTransaction CreateTransaction(ConnectionScope connectionScope)
        {
            if (TransactionScopesMatching(connectionScope).Any())
                return TransactionScopesMatching(connectionScope).First().Transaction;

            return connectionScope.BeginTransaction();
        }

        public void EndTransaction(TransactionScope transactionScope)
        {
            _liveTransactionScopes.Remove(transactionScope);

            if (TransactionScopesMatching(transactionScope).IsEmpty())
            {
                EndConnection(transactionScope.ConnectionScope);
                transactionScope.Transaction.Dispose();
            }
        }

        private IEnumerable<TransactionScope> TransactionScopesMatching(TransactionScope transactionScope)
        {
            return _liveTransactionScopes.Where(x => x.Matches(transactionScope));
        }

        private IEnumerable<TransactionScope> TransactionScopesMatching(ConnectionScope connectionScope)
        {
            return _liveTransactionScopes.Where(x => x.Matches(connectionScope));
        }

        public DbCommand CreateCommand(string commandText)
        {
            return CreateNewCommand(commandText);
        }

        protected abstract DbCommand CreateNewCommand(string commandText);
        public abstract DbParameter CreateParameter(string name, object value);
        protected abstract DbConnection CreateNewConnection(string connectionString);

        public virtual DbConnection CreateConnection(string connectionString)
        {
            var connectionScopesForConnectionString = ConnectionScopesMatching(connectionString);

            if (connectionScopesForConnectionString.Any())
                return connectionScopesForConnectionString.First().Connection;

            return CreateNewConnection(connectionString);
        }

        private List<ConnectionScope> ConnectionScopesMatching(ConnectionScope connectionScope)
        {
            return _liveConnectionScopes.Where(x => x.Matches(connectionScope.Connection.ConnectionString)).ToList();
        }
        
        private List<ConnectionScope> ConnectionScopesMatching(string connectionString)
        {
            return _liveConnectionScopes.Where(x => x.Matches(connectionString)).ToList();
        }

        public virtual DbParameter CreateParameter(CommandParameter commandParameter)
        {
            return CreateParameter(commandParameter.Name, commandParameter.Value);
        }

        public virtual DbCommand CreateCommand()
        {
            return CreateCommand(null);
        }
    }
}