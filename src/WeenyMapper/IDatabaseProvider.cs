using WeenyMapper.Sql;

namespace WeenyMapper
{
    public interface IDatabaseProvider
    {
        TSqlGenerator CreateSqlGenerator(IDbCommandFactory dbCommandFactory);
        IDbCommandFactory CreateDbCommandFactory(string connectionString);
    }
}