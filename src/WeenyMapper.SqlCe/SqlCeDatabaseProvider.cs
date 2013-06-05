using WeenyMapper.Sql;

namespace WeenyMapper.SqlCe
{
    public class SqlCeDatabaseProvider : IDatabaseProvider
    {
        public TSqlGenerator CreateSqlGenerator(IDbCommandFactory dbCommandFactory)
        {
            return new SqlCeTSqlGenerator(dbCommandFactory);
        }

        public IDbCommandFactory CreateDbCommandFactory(string connectionString)
        {
            return new SqlCeCommandFactory(connectionString);
        }
    }
}