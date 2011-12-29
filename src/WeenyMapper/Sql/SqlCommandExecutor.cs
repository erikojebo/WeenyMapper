using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using WeenyMapper.Logging;

namespace WeenyMapper.Sql
{
    public class SqlCommandExecutor : IDbCommandExecutor
    {
        private readonly ISqlCommandLogger _sqlCommandLogger;

        public SqlCommandExecutor(ISqlCommandLogger sqlCommandLogger)
        {
            _sqlCommandLogger = sqlCommandLogger;
        }

        public void ExecuteNonQuery(DbCommand command, string connectionString)
        {
            ExecuteNonQuery(new[] { command }, connectionString);
        }

        public void ExecuteNonQuery(IEnumerable<DbCommand> command, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var dbCommand in command)
                {
                    _sqlCommandLogger.Log(dbCommand);

                    dbCommand.Connection = connection;
                    dbCommand.ExecuteNonQuery();
                    dbCommand.Dispose();
                }
            }
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