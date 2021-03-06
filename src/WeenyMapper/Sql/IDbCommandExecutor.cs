using System;
using System.Collections.Generic;
using System.Data.Common;
using WeenyMapper.Mapping;

namespace WeenyMapper.Sql
{
    public interface IDbCommandExecutor
    {
        T ExecuteScalar<T>(DbCommand command, string connectionString);
        IList<T> ExecuteScalarList<T>(DbCommand command, string connectionString);
        IList<T> ExecuteScalarList<T>(IEnumerable<DbCommand> commands, string connectionString);
        IList<T> ExecuteScalarList<T>(IEnumerable<ScalarCommand> commands, string connectionString);

        int ExecuteNonQuery(DbCommand command, string connectionString);
        IList<int> ExecuteNonQuery(IEnumerable<DbCommand> commands, string connectionString);

        IList<T> ExecuteQuery<T>(DbCommand command, Func<IDictionary<string, object>, T> resultReader, string connectionString);
        ResultSet ExecuteQuery(DbCommand command, string connectionString);
    }
}