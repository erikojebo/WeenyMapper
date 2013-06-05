using System.Data.Common;
using WeenyMapper.Sql;

namespace WeenyMapper.Specs.Sql
{
    public class TestDbCommandFactory : DbCommandFactoryBase
    {
        public TestDbCommandFactory(string connectionString) : base(connectionString)
        {
        }

        protected override DbCommand CreateNewCommand(string commandText)
        {
            return new TestDbCommand { CommandText = commandText };
        }

        public override DbParameter CreateParameter(string name, object value)
        {
            return new TestDbParameter { ParameterName = name, Value = value };
        }

        protected override DbConnection CreateNewConnection(string connectionString)
        {
            return new TestDbConnection { ConnectionString = connectionString };
        }
    }
}