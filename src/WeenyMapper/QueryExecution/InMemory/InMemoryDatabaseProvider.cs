using System.Data.Common;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution.InMemory
{
    public class InMemoryDatabaseProvider : IDatabaseProvider
    {
        private readonly InMemoryDatabase _inMemoryDatabase;

        public InMemoryDatabaseProvider(InMemoryDatabase inMemoryDatabase)
        {
            _inMemoryDatabase = inMemoryDatabase;
        }

        public TSqlGenerator CreateSqlGenerator(IDbCommandFactory dbCommandFactory)
        {
            throw new System.NotImplementedException();
        }

        public IDbCommandFactory CreateDbCommandFactory(string connectionString)
        {
            return new InMemoryDbCommandFactory(_inMemoryDatabase);
        }
    }

    public class InMemoryDbCommandFactory : IDbCommandFactory
    {
        private readonly InMemoryDatabase _inMemoryDatabase;

        public InMemoryDbCommandFactory(InMemoryDatabase inMemoryDatabase)
        {
            _inMemoryDatabase = inMemoryDatabase;
        }

        public void Dispose()
        {
        }

        public DbCommand CreateCommand()
        {
            throw new System.NotImplementedException();
        }

        public DbCommand CreateCommand(string commandText)
        {
            throw new System.NotImplementedException();
        }

        public DbParameter CreateParameter(string name, object value)
        {
            throw new System.NotImplementedException();
        }

        public DbParameter CreateParameter(CommandParameter commandParameter)
        {
            throw new System.NotImplementedException();
        }

        public DbConnection CreateConnection()
        {
            throw new System.NotImplementedException();
        }

        public ConnectionScope BeginConnection()
        {
            return new ConnectionScope(this, null);
        }

        public void EndConnection(ConnectionScope connectionScope)
        {
        }

        public TransactionScope BeginTransaction()
        {
            return _inMemoryDatabase.BeginTransaction();
        }

        public void EndTransaction(TransactionScope transactionScope)
        {
            // The transaction is ended directly in the InMemoryDatabase
        }

        public bool Matches(string connectionString)
        {
            return true;
        }
    }
}