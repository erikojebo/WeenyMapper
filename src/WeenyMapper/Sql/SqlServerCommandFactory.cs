using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace WeenyMapper.Sql
{
    public class SqlServerCommandFactory : IDbCommandFactory
    {
        public DbCommand CreateCommand(string commandText)
        {
            return new SqlCommand(commandText);
        }

        public DbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        public DbParameter CreateParameter(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public DbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public DbConnection CreateConnection()
        {
            return new SqlConnection();
        }
    }
}