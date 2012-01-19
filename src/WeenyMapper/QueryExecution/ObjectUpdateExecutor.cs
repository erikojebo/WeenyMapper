using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Conventions;
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
        private readonly IConvention _convention;

        public ObjectUpdateExecutor(ISqlGenerator sqlGenerator, IConventionDataReader conventionDataReader, IDbCommandExecutor dbCommandExecutor, IConvention convention)
        {
            _sqlGenerator = sqlGenerator;
            _conventionDataReader = conventionDataReader;
            _dbCommandExecutor = dbCommandExecutor;
            _convention = convention;
        }

        public string ConnectionString { get; set; }

        public int Update<T>(T instance)
        {
            var tableName = _conventionDataReader.GetTableName<T>();
            var columnValues = _conventionDataReader.GetColumnValuesFromEntity(instance);

            var primaryKeyColumn = _conventionDataReader.GetPrimaryKeyColumnName<T>();
            var primaryKeyValue = _conventionDataReader.GetPrimaryKeyValue(instance);

            var constraintExpression = QueryExpression.Create(new EqualsExpression(primaryKeyColumn, primaryKeyValue));

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, constraintExpression, columnValues);

            return _dbCommandExecutor.ExecuteNonQuery(command, ConnectionString);
        }

        public int Update<T>(QueryExpression queryExpression, IDictionary<string, object> setters)
        {
            var tableName = _conventionDataReader.GetTableName<T>();
            var columnSetters = _conventionDataReader.GetColumnValues(setters);
            var primaryKeyColumn = _conventionDataReader.GetPrimaryKeyColumnName<T>();

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, queryExpression.Translate(_convention), columnSetters);

            return _dbCommandExecutor.ExecuteNonQuery(command, ConnectionString);
        }
    }
}