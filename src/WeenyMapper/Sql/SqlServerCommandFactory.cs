using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace WeenyMapper.Sql
{
    public class SqlServerCommandFactory : DbCommandFactoryBase
    {
        public SqlServerCommandFactory(string connectionString) : base(connectionString)
        {
        }

        protected override DbCommand CreateNewCommand(string commandText)
        {
            return new SqlCommand(commandText);
        }

        public override DbParameter CreateParameter(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        protected override DbConnection CreateNewConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}