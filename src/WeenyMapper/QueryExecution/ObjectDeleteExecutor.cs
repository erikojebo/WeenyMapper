using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectDeleteExecutor : IObjectDeleteExecutor
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;

        public ObjectDeleteExecutor(IConvention convention, ISqlGenerator sqlGenerator)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
        }

        public string ConnectionString { get; set; }

        public void Delete<T>(T instance)
        {
            var className = typeof(T).Name;
            var tableName = _convention.GetTableName(className);

            var constraints = new Dictionary<string, object>();

            var primaryKeyColumnName = GetPrimaryKeyColumn<T>();
            var primaryKeyValue = GetPrimaryKeyValue(instance);

            constraints[primaryKeyColumnName] = primaryKeyValue;

            var command = _sqlGenerator.CreateDeleteCommand(tableName, constraints);

            command.ExecuteNonQuery(ConnectionString);
        }

        public void Delete<T>(IDictionary<string, object> constraints)
        {
            var className = typeof(T).Name;
            var tableName = _convention.GetTableName(className);

            var columnConstraints = constraints.TransformKeys(_convention.GetColumnName);
            var command = _sqlGenerator.CreateDeleteCommand(tableName, columnConstraints);

            command.ExecuteNonQuery(ConnectionString);
        }

        private string GetPrimaryKeyColumn<T>()
        {
            var propertyName = GetIdProperty<T>().Name;
            return _convention.GetColumnName(propertyName);
        }

        private PropertyInfo GetIdProperty<T>()
        {
            return typeof(T).GetProperties()
                .First(x => _convention.IsIdProperty(x.Name));
        }

        private object GetPrimaryKeyValue<T>(T instance)
        {
            var property = GetIdProperty<T>();
            return property.GetValue(instance, null);
        }
    }
}