using System.Data.Common;

namespace WeenyMapper.Sql
{
    public interface IDbCommandFactory
    {
        DbCommand CreateCommand();
        DbCommand CreateCommand(string commandText);
        DbParameter CreateParameter(string name, object value);
        DbConnection CreateConnection();
        DbConnection CreateConnection(string connectionString);
    }
}