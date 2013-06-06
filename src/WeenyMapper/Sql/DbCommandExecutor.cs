using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using WeenyMapper.Logging;
using WeenyMapper.Mapping;

namespace WeenyMapper.Sql
{
    public class DbCommandExecutor : IDbCommandExecutor
    {
        private readonly ISqlCommandLogger _sqlCommandLogger;
        private readonly IDbCommandFactory _commandFactory;

        public DbCommandExecutor(ISqlCommandLogger sqlCommandLogger, IDbCommandFactory commandFactory)
        {
            _sqlCommandLogger = sqlCommandLogger;
            _commandFactory = commandFactory;
        }

        public int ExecuteNonQuery(DbCommand command, string connectionString)
        {
            var rowCounts = ExecuteNonQuery(new[] { command }, connectionString);
            return rowCounts.First();
        }

        public IList<int> ExecuteNonQuery(IEnumerable<DbCommand> commands, string connectionString)
        {
            return WithConnection(connection => commands
                                                    .Select(command => ExecuteNonQuery(command, connection))
                                                    .ToList());
        }

        private int ExecuteNonQuery(DbCommand command, DbConnection connection)
        {
            _sqlCommandLogger.Log(command);

            command.Connection = connection;
            var rowCount = command.ExecuteNonQuery();
            command.Dispose();

            return rowCount;
        }

        public IList<T> ExecuteQuery<T>(DbCommand command, Func<IDictionary<string, object>, T> resultReader, string connectionString)
        {
            var results = new List<T>();

            ExecuteQuery(command, reader =>
                {
                    var propertyValues = GetValues(reader);
                    var result = resultReader(propertyValues);
                    results.Add(result);
                });

            return results;
        }

        public ResultSet ExecuteQuery(DbCommand command, string connectionString)
        {
            var resultSet = new ResultSet();

            ExecuteQuery(command, reader =>
                {
                    var propertyValues = GetValues(reader).Select(x => new ColumnValue(x.Key, x.Value));
                    resultSet.AddRow(propertyValues);
                });

            return resultSet;
        }

        private void ExecuteQuery(DbCommand command, Action<DbDataReader> readAction)
        {
            WithConnection(connection =>
                {
                    command.Connection = connection;

                    _sqlCommandLogger.Log(command);

                    using (var dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            readAction(dataReader);
                        }
                    }

                    command.Dispose();

                    return 0;
                });
        }

        public IList<T> ExecuteScalarList<T>(IEnumerable<ScalarCommand> commands, string connectionString)
        {
            return WithConnection(connection => commands
                                                    .Select(command => ExecuteScalarCommand<T>(command, connection))
                                                    .ToList());
        }

        private T ExecuteScalarCommand<T>(ScalarCommand scalarCommand, DbConnection connection)
        {
            foreach (var preparatoryCommand in scalarCommand.PreparatoryCommands)
            {
                ExecuteNonQuery(preparatoryCommand, connection);
            }

            return ExecuteScalar<T>(scalarCommand.ResultCommand, connection);
        }

        public T ExecuteScalar<T>(DbCommand command, string connectionString)
        {
            return WithConnection(connection => ExecuteScalar<T>(command, connection));
        }

        public IList<T> ExecuteScalarList<T>(IEnumerable<DbCommand> commands, string connectionString)
        {
            return WithConnection(connection => commands
                                                    .Select(dbCommand => ExecuteScalar<T>(dbCommand, connection))
                                                    .ToList());
        }

        private T ExecuteScalar<T>(DbCommand command, DbConnection connection)
        {
            _sqlCommandLogger.Log(command);

            command.Connection = connection;
            T resultScalar = (T)command.ExecuteScalar();
            command.Dispose();

            return resultScalar;
        }

        public IList<T> ExecuteScalarList<T>(DbCommand command, string connectionString)
        {
            return WithConnection(connection =>
                {
                    var result = new List<T>();

                    command.Connection = connection;

                    _sqlCommandLogger.Log(command);

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var value = reader.GetValue(0);
                        result.Add((T)value);
                    }

                    command.Dispose();

                    return result;
                });
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

        private List<T> WithConnection<T>(Func<DbConnection, List<T>> func)
        {
            return WithConnection<List<T>>(func);
        }

        private T WithConnection<T>(Func<DbConnection, T> func)
        {
            var connection = _commandFactory.CreateConnection();
            T result;

            var wasOpenedManually = false;

            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                    wasOpenedManually = true;
                }

                result = func(connection);
            }
            finally
            {
                if (wasOpenedManually)
                    connection.Dispose();
            }

            return result;
        }
    }
}