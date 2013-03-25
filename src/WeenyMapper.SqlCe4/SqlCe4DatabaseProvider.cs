using WeenyMapper.Sql;
using WeenyMapper.SqlCe4.Sql;

namespace WeenyMapper.SqlCe4
{
    public class SqlCe4DatabaseProvider : IDatabaseProvider
    {
        public TSqlGenerator CreateSqlGenerator()
        {
            return new SqlCe4TSqlGenerator(CreateDbCommandFactory());
        }

        public IDbCommandFactory CreateDbCommandFactory()
        {
            return new SqlCe4CommandFactory();
        }
    }
}