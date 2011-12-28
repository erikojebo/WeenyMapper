using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Extensions;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectUpdateExecutor : IObjectUpdateExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IConventionalEntityDataReader _entityDataReader;

        public ObjectUpdateExecutor(ISqlGenerator sqlGenerator, IConventionalEntityDataReader entityDataReader)
        {
            _sqlGenerator = sqlGenerator;
            _entityDataReader = entityDataReader;
        }

        public string ConnectionString { get; set; }

        public void Update<T>(T instance)
        {
            var tableName = _entityDataReader.GetTableName<T>();
            var columnValues = _entityDataReader.GetColumnValuesFromEntity(instance);

            var primaryKeyColumn = _entityDataReader.GetPrimaryKeyColumnName<T>();

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, columnValues);

            command.ExecuteNonQuery(ConnectionString);
        }

        public void Update<T>(IDictionary<string, object> constraints, IDictionary<string, object> setters)
        {
            var tableName = _entityDataReader.GetTableName<T>();
            var columnConstraints = _entityDataReader.GetColumnValues(constraints);
            var columnSetters = _entityDataReader.GetColumnValues(setters);

            var primaryKeyColumn = _entityDataReader.GetPrimaryKeyColumnName<T>();

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, columnConstraints, columnSetters);

            command.ExecuteNonQuery(ConnectionString);
        }
    }
}