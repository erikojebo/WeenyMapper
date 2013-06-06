using System;
using System.Data;
using System.Data.Common;
using WeenyMapper.Sql;

namespace WeenyMapper
{
    public class ConnectionScope : IDisposable
    {
        private readonly IDbCommandFactory _dbCommandFactory;

        public ConnectionScope(IDbCommandFactory dbCommandFactory, DbConnection connection)
        {
            Connection = connection;
            _dbCommandFactory = dbCommandFactory;
        }

        public DbConnection Connection { get; set; }

        public void Dispose()
        {
            _dbCommandFactory.EndConnection(this);
        }

        public bool Matches(ConnectionScope connectionScope)
        {
            return Matches(connectionScope.Connection.ConnectionString);
        }

        public bool Matches(string connectionString)
        {
            return Connection.ConnectionString.ToLower() == connectionString.ToLower();
        }

        public DbTransaction BeginTransaction()
        {
            return Connection.BeginTransaction();
        }

        public void DisposeConnection()
        {
            if (Connection.State == ConnectionState.Open)
                Connection.Close();

            Connection.Dispose();
        }
    }
}