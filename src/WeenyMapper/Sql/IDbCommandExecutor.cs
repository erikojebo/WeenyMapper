using System.Collections.Generic;
using System.Data.Common;

namespace WeenyMapper.Sql
{
    public interface IDbCommandExecutor
    {
        void ExecuteNonQuery(DbCommand command, string connectionString);
        void ExecuteNonQuery(IEnumerable<DbCommand> command, string connectionString);
    }
}