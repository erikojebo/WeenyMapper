using System;
using System.Data.Common;
using System.Data.SqlServerCe;

namespace WeenyMapper.Sql
{
    public class SqlCeCommandFactory : DbCommandFactoryBase
    {
        public override DbCommand CreateCommand(string commandText)
        {
            return new SqlCeCommand(commandText);
        }

        public override DbParameter CreateParameter(string name, object value)
        {
            return new SqlCeParameter(name, value ?? DBNull.Value);
        }

        protected override DbConnection CreateNewConnection(string connectionString)
        {
            return new SqlCeConnection(connectionString);
        }
    }
}