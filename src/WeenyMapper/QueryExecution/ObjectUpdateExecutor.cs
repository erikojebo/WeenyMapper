using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Extensions;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectUpdateExecutor : IObjectUpdateExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IConventionDataReader _conventionDataReader;
        private readonly IDbCommandExecutor _dbCommandExecutor;

        public ObjectUpdateExecutor(ISqlGenerator sqlGenerator, IConventionDataReader conventionDataReader, IDbCommandExecutor dbCommandExecutor)
        {
            _sqlGenerator = sqlGenerator;
            _conventionDataReader = conventionDataReader;
            _dbCommandExecutor = dbCommandExecutor;
        }

        public string ConnectionString { get; set; }

        public int Update<T>(T instance)
        {
            var tableName = _conventionDataReader.GetTableName<T>();
            var columnValues = _conventionDataReader.GetColumnValuesFromEntity(instance);

            var primaryKeyColumn = _conventionDataReader.GetPrimaryKeyColumnName<T>();

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, columnValues);

            return _dbCommandExecutor.ExecuteNonQuery(command, ConnectionString);
        }

        public int Update<T>(IDictionary<string, object> constraints, IDictionary<string, object> setters)
        {
            var tableName = _conventionDataReader.GetTableName<T>();
            var columnConstraints = _conventionDataReader.GetColumnValues(constraints);
            var columnSetters = _conventionDataReader.GetColumnValues(setters);

            var primaryKeyColumn = _conventionDataReader.GetPrimaryKeyColumnName<T>();

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, columnConstraints, columnSetters);

            return _dbCommandExecutor.ExecuteNonQuery(command, ConnectionString);
        }

        public int Update<T>(QueryExpression queryExpression, IDictionary<string, object> setters)
        {
            var tableName = _conventionDataReader.GetTableName<T>();
            var columnSetters = _conventionDataReader.GetColumnValues(setters);
            var primaryKeyColumn = _conventionDataReader.GetPrimaryKeyColumnName<T>();

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, queryExpression, columnSetters);

            return _dbCommandExecutor.ExecuteNonQuery(command, ConnectionString);
        }
    }
}