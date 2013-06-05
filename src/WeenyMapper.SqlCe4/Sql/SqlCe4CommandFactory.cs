using System;
using System.Data.Common;
using System.Data.SqlServerCe;
using WeenyMapper.Sql;

namespace WeenyMapper.SqlCe4.Sql
{
    public class SqlCe4CommandFactory : DbCommandFactoryBase
    {
        public SqlCe4CommandFactory(string connectionString) : base(connectionString)
        {
        }

        protected override DbCommand CreateNewCommand(string commandText)
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