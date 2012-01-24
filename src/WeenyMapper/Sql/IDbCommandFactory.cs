using System.Data.Common;

namespace WeenyMapper.Sql
{
    public interface IDbCommandFactory {
        DbCommand CreateCommand();
        DbParameter CreateParameter();
        DbConnection CreateConnection();
    }
}