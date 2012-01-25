using WeenyMapper.Sql;

namespace WeenyMapper.SqlCe
{
    public class SqlCeDatabaseProvider : IDatabaseProvider
    {
        public TSqlGenerator CreateSqlGenerator()
        {
            return new SqlCeTSqlGenerator(CreateDbCommandFactory());
        }

        public IDbCommandFactory CreateDbCommandFactory()
        {
            return new SqlCeCommandFactory();
        }
    }
}