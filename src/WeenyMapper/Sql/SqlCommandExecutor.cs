using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;

namespace WeenyMapper.Sql
{
    public class SqlCommandExecutor : IDbCommandExecutor
    {
        public void ExecuteNonQuery(DbCommand command, string connectionString)
        {
            ExecuteNonQuery(new [] { command }, connectionString);
        }

        public void ExecuteNonQuery(IEnumerable<DbCommand> command, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var dbCommand in command)
                {
                    dbCommand.Connection = connection;
                    dbCommand.ExecuteNonQuery();
                    dbCommand.Dispose();
                }
            }
        }

        public IList<T> ExecuteQuery<T>(DbCommand command, Func<DbDataReader, T> resultReader,  string connectionString)
        {
            var results = new List<T>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                command.Connection = connection;

                var dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {
                    var result = resultReader(dataReader);
                    results.Add(result);
                }

                command.Dispose();
            }

            return results;
        }
    }
}