using WeenyMapper.Sql;

namespace WeenyMapper
{
    public class SqlServerDatabaseProvider : IDatabaseProvider
    {
        public TSqlGenerator CreateSqlGenerator(IDbCommandFactory dbCommandFactory)
        {
            return new TSqlGenerator(dbCommandFactory);
        }

        public IDbCommandFactory CreateDbCommandFactory(string connectionString)
        {
            return new SqlServerCommandFactory(connectionString);
        }
    }
}