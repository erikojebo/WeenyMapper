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

        public ConnectionScope BeginConnection(string connectionString)
        {
            var connection = CreateConnection(connectionString);

            if (connection.State == ConnectionState.Closed)
                connection.Open();

            var connectionScope = new ConnectionScope(this, connection);

            _liveConnectionScopes.Add(connectionScope);

            return connectionScope;
        }

        public void EndConnection(ConnectionScope connectionScope)
        {
            _liveConnectionScopes.Remove(connectionScope);

            if (ConnectionScopesMatching(connectionScope).IsEmpty())
            {
                if (connectionScope.Connection.State == ConnectionState.Open)
                    connectionScope.Connection.Close();

                connectionScope.Connection.Dispose();
            }
        }

        public abstract DbCommand CreateCommand(string commandText);
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