using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using WeenyMapper.Logging;
using System.Linq;

namespace WeenyMapper.Sql
{
    public class SqlCommandExecutor : IDbCommandExecutor
    {
        private readonly ISqlCommandLogger _sqlCommandLogger;

        public SqlCommandExecutor(ISqlCommandLogger sqlCommandLogger)
        {
            _sqlCommandLogger = sqlCommandLogger;
        }

        public int ExecuteNonQuery(DbCommand command, string connectionString)
        {
            var rowCounts = ExecuteNonQuery(new[] { command }, connectionString);
            return rowCounts.First();
        }

        public IList<int> ExecuteNonQuery(IEnumerable<DbCommand> command, string connectionString)
        {
            var rowCounts = new List<int>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var dbCommand in command)
                {
                    _sqlCommandLogger.Log(dbCommand);

                    dbCommand.Connection = connection;
                    var rowCount = dbCommand.ExecuteNonQuery();
                    dbCommand.Dispose();

                    rowCounts.Add(rowCount);
                }
            }

            return rowCounts;
        }

        public IList<T> ExecuteQuery<T>(DbCommand command, Func<DbDataReader, T> resultReader, string connectionString)
        {
            var results = new List<T>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                command.Connection = connection;

                _sqlCommandLogger.Log(command);

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

        public T ExecuteScalar<T>(DbCommand command, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                command.Connection = connection;

                _sqlCommandLogger.Log(command);

                T result = (T)command.ExecuteScalar();
                command.Dispose();

                return result;
            }
        }
    }
}