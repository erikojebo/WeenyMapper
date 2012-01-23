using System;
using System.Collections.Generic;
using System.Data.Common;

namespace WeenyMapper.Sql
{
    public interface IDbCommandExecutor
    {
        T ExecuteScalar<T>(DbCommand command, string connectionString);
        IList<T> ExecuteScalarList<T>(DbCommand command, string connectionString);
        IList<T> ExecuteScalarList<T>(IEnumerable<DbCommand> commands, string connectionString);

        int ExecuteNonQuery(DbCommand command, string connectionString);
        IList<T> ExecuteQuery<T>(DbCommand command, Func<IDictionary<string, object >, T> resultReader,  string connectionString);

        IList<int> ExecuteNonQuery(IEnumerable<DbCommand> commands, string connectionString);
    }
}