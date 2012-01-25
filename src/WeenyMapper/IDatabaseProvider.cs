using WeenyMapper.Sql;

namespace WeenyMapper
{
    public interface IDatabaseProvider
    {
        TSqlGenerator CreateSqlGenerator();
        IDbCommandFactory CreateDbCommandFactory();
    }
}