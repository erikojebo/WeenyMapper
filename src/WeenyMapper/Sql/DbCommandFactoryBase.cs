using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace WeenyMapper.Sql
{
    public abstract class DbCommandFactoryBase : IDbCommandFactory
    {
        private readonly IList<ConnectionScope> _liveConnectionScopes = new List<ConnectionScope>();

        public ConnectionScope BeginConnection(string connectionString)
        {
            var connectionScope = new ConnectionScope(this, CreateConnection(connectionString));

            _liveConnectionScopes.Add(connectionScope);

            return connectionScope;
        }

        public void EndConnection(ConnectionScope connectionScope)
        {
            _liveConnectionScopes.Remove(connectionScope);
        }

        public abstract DbCommand CreateCommand(string commandText);
        public abstract DbParameter CreateParameter(string name, object value);
        protected abstract DbConnection CreateNewConnection(string connectionString);

        public virtual DbConnection CreateConnection(string connectionString)
        {
            var connectionScopesForConnectionString = _liveConnectionScopes.Where(x => x.Matches(connectionString)).ToList();

            if (connectionScopesForConnectionString.Any())
                return connectionScopesForConnectionString.First().Connection;

            return CreateNewConnection(connectionString);
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