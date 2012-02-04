using System;
using System.Data.Common;
using System.Data.SqlServerCe;

namespace WeenyMapper.Sql
{
    public class SqlCeCommandFactory : IDbCommandFactory
    {
        public DbCommand CreateCommand(string commandText)
        {
            return new SqlCeCommand(commandText);
        }

        public DbCommand CreateCommand()
        {
            return new SqlCeCommand();
        }

        public DbParameter CreateParameter(string name, object value)
        {
            return new SqlCeParameter(name, value ?? DBNull.Value);
        }

        public DbParameter CreateParameter(CommandParameter commandParameter)
        {
            return CreateParameter(commandParameter.Name, commandParameter.Value);
        }

        public DbConnection CreateConnection(string connectionString)
        {
            return new SqlCeConnection(connectionString);
        }

        public DbConnection CreateConnection()
        {
            return new SqlCeConnection();
        }
    }
}