using WeenyMapper.Sql;

namespace WeenyMapper
{
    public class SqlServerDatabaseProvider : IDatabaseProvider
    {
        public TSqlGenerator CreateSqlGenerator(string connectionString)
        {
            return new TSqlGenerator(CreateDbCommandFactory(connectionString));
        }

        public IDbCommandFactory CreateDbCommandFactory(string connectionString)
        {
            return new SqlServerCommandFactory(connectionString);
        }
    }
}