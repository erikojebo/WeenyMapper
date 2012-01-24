using System.Data.Common;
using System.Data.SqlClient;

namespace WeenyMapper.Sql
{
    public class SqlServerCommandFactory : IDbCommandFactory
    {
        public DbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        public DbParameter CreateParameter()
        {
            return new SqlParameter();
        }

        public DbConnection CreateConnection()
        {
            return new SqlConnection();
        }
    }
}