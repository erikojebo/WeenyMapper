using System;
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

        public bool Matches(string connectionString)
        {
            return Connection.ConnectionString.ToLower() == connectionString.ToLower();
        }
    }
}