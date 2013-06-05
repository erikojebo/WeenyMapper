using WeenyMapper.Sql;

namespace WeenyMapper
{
    public interface IDatabaseProvider
    {
        TSqlGenerator CreateSqlGenerator(string connectionString);
        IDbCommandFactory CreateDbCommandFactory(string connectionString);
    }
}