using WeenyMapper.Sql;

namespace WeenyMapper.SqlCe
{
    public class SqlCeDatabaseProvider : IDatabaseProvider
    {
        public TSqlGenerator CreateSqlGenerator(string connectionString)
        {
            return new SqlCeTSqlGenerator(CreateDbCommandFactory(connectionString));
        }

        public IDbCommandFactory CreateDbCommandFactory(string connectionString)
        {
            return new SqlCeCommandFactory(connectionString);
        }
    }
}