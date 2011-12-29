using System;
using System.Collections.Generic;
using System.Data.Common;

namespace WeenyMapper.Sql
{
    public interface IDbCommandExecutor
    {
        int ExecuteNonQuery(DbCommand command, string connectionString);
        IList<int> ExecuteNonQuery(IEnumerable<DbCommand> command, string connectionString);
        IList<T> ExecuteQuery<T>(DbCommand command, Func<DbDataReader, T> resultReader,  string connectionString);
        T ExecuteScalar<T>(DbCommand command, string connectionString);
    }
}