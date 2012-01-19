using System;
using System.Collections.Generic;
using WeenyMapper.Async;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectDeleteExecutor : IObjectDeleteExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IConventionDataReader _conventionDataReader;
        private readonly IDbCommandExecutor _dbCommandExecutor;
        private readonly IConvention _convention;

        public ObjectDeleteExecutor(ISqlGenerator sqlGenerator, IConventionDataReader conventionDataReader, IDbCommandExecutor dbCommandExecutor, IConvention convention)
        {
            _sqlGenerator = sqlGenerator;
            _conventionDataReader = conventionDataReader;
            _dbCommandExecutor = dbCommandExecutor;
            _convention = convention;
        }

        public string ConnectionString { get; set; }

        public int Delete<T>(T instance)
        {
            var tableName = _conventionDataReader.GetTableName<T>();

            var constraints = new Dictionary<string, object>();

            var primaryKeyColumnName = _conventionDataReader.GetPrimaryKeyColumnName<T>();
            var primaryKeyValue = _conventionDataReader.GetPrimaryKeyValue(instance);

            constraints[primaryKeyColumnName] = primaryKeyValue;

            var command = _sqlGenerator.CreateDeleteCommand(tableName, constraints);

            return _dbCommandExecutor.ExecuteNonQuery(command, ConnectionString);
        }

        public int Delete<T>(IDictionary<string, object> constraints)
        {
            var tableName = _conventionDataReader.GetTableName<T>();
            var columnConstraints = _conventionDataReader.GetColumnValues(constraints);
            var command = _sqlGenerator.CreateDeleteCommand(tableName, columnConstraints);

            return _dbCommandExecutor.ExecuteNonQuery(command, ConnectionString);
        }

        public void DeleteAsync<T>(T entity, Action callback)
        {
            TaskRunner.Run(() => Delete(entity), callback);
        }

        public int Delete<T>(QueryExpression queryExpression)
        {
            var tableName = _conventionDataReader.GetTableName<T>();
            var command = _sqlGenerator.CreateDeleteCommand(tableName, queryExpression.Translate(_convention));

            return _dbCommandExecutor.ExecuteNonQuery(command, ConnectionString);
        }
    }
}