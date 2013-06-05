using WeenyMapper.Sql;
using WeenyMapper.SqlCe4.Sql;

namespace WeenyMapper.SqlCe4
{
    public class SqlCe4DatabaseProvider : IDatabaseProvider
    {
        public TSqlGenerator CreateSqlGenerator(IDbCommandFactory dbCommandFactory)
        {
            return new SqlCe4TSqlGenerator(dbCommandFactory);
        }

        public IDbCommandFactory CreateDbCommandFactory(string connectionString)
        {
            return new SqlCe4CommandFactory(connectionString);
        }
    }
}