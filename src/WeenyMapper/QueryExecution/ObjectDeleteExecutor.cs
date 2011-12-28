using System.Collections.Generic;
using WeenyMapper.Extensions;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectDeleteExecutor : IObjectDeleteExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IConventionDataReader _conventionDataReader;

        public ObjectDeleteExecutor(ISqlGenerator sqlGenerator, IConventionDataReader conventionDataReader)
        {
            _sqlGenerator = sqlGenerator;
            _conventionDataReader = conventionDataReader;
        }

        public string ConnectionString { get; set; }

        public void Delete<T>(T instance)
        {
            var tableName = _conventionDataReader.GetTableName<T>();

            var constraints = new Dictionary<string, object>();

            var primaryKeyColumnName = _conventionDataReader.GetPrimaryKeyColumnName<T>();
            var primaryKeyValue = _conventionDataReader.GetPrimaryKeyValue(instance);

            constraints[primaryKeyColumnName] = primaryKeyValue;

            var command = _sqlGenerator.CreateDeleteCommand(tableName, constraints);

            command.ExecuteNonQuery(ConnectionString);
        }

        public void Delete<T>(IDictionary<string, object> constraints)
        {
            var tableName = _conventionDataReader.GetTableName<T>();
            var columnConstraints = _conventionDataReader.GetColumnValues(constraints);
            var command = _sqlGenerator.CreateDeleteCommand(tableName, columnConstraints);

            command.ExecuteNonQuery(ConnectionString);
        }
    }
}