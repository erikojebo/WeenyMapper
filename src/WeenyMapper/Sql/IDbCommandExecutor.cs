using System;
using System.Collections.Generic;
using System.Data.Common;

namespace WeenyMapper.Sql
{
    public interface IDbCommandExecutor
    {
        void ExecuteNonQuery(DbCommand command, string connectionString);
        void ExecuteNonQuery(IEnumerable<DbCommand> command, string connectionString);
        IList<T> ExecuteQuery<T>(DbCommand command, Func<DbDataReader, T> resultReader,  string connectionString);
    }
}