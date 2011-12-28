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
        private readonly IConventionDataReader _conventionDataReader;

        public ObjectUpdateExecutor(ISqlGenerator sqlGenerator, IConventionDataReader conventionDataReader)
        {
            _sqlGenerator = sqlGenerator;
            _conventionDataReader = conventionDataReader;
        }

        public string ConnectionString { get; set; }

        public void Update<T>(T instance)
        {
            var tableName = _conventionDataReader.GetTableName<T>();
            var columnValues = _conventionDataReader.GetColumnValuesFromEntity(instance);

            var primaryKeyColumn = _conventionDataReader.GetPrimaryKeyColumnName<T>();

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, columnValues);

            command.ExecuteNonQuery(ConnectionString);
        }

        public void Update<T>(IDictionary<string, object> constraints, IDictionary<string, object> setters)
        {
            var tableName = _conventionDataReader.GetTableName<T>();
            var columnConstraints = _conventionDataReader.GetColumnValues(constraints);
            var columnSetters = _conventionDataReader.GetColumnValues(setters);

            var primaryKeyColumn = _conventionDataReader.GetPrimaryKeyColumnName<T>();

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, columnConstraints, columnSetters);

            command.ExecuteNonQuery(ConnectionString);
        }
    }
}