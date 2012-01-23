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

        public IList<int> ExecuteNonQuery(IEnumerable<DbCommand> commands, string connectionString)
        {
            var rowCounts = new List<int>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var dbCommand in commands)
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

        public IList<T> ExecuteQuery<T>(DbCommand command, Func<IDictionary<string, object>, T> resultReader, string connectionString)
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
                    var propertyValues = GetValues(dataReader);
                    var result = resultReader(propertyValues);
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

        public IList<T> ExecuteScalarList<T>(DbCommand command, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var result = new List<T>();

                connection.Open();

                command.Connection = connection;

                _sqlCommandLogger.Log(command);

                var reader = command.ExecuteReader();

                while(reader.Read())
                {
                    var value = reader.GetValue(0);
                    result.Add((T)value);
                }

                command.Dispose();

                return result;
            }
        }

        public IList<T> ExecuteScalarList<T>(IEnumerable<DbCommand> commands, string connectionString)
        {
            var values = new List<T>();

            foreach (var dbCommand in commands)
            {
                var value = ExecuteScalar<T>(dbCommand, connectionString);
                values.Add(value);
            }

            return values;
        }

        private Dictionary<string, object> GetValues(DbDataReader reader)
        {
            var values = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.GetValue(i);

                values[name] = value;
            }
            return values;
        }

    }
}