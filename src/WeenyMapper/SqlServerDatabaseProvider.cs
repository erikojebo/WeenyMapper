using WeenyMapper.Sql;

namespace WeenyMapper
{
    public class SqlServerDatabaseProvider : IDatabaseProvider
    {
        public TSqlGenerator CreateSqlGenerator()
        {
            return new TSqlGenerator(CreateDbCommandFactory());
        }

        public IDbCommandFactory CreateDbCommandFactory()
        {
            return new SqlServerCommandFactory();
        }
    }
}