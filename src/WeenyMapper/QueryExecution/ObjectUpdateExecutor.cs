using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectUpdateExecutor : IObjectUpdateExecutor
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IPropertyReader _propertyReader;

        public ObjectUpdateExecutor(IConvention convention, ISqlGenerator sqlGenerator, IPropertyReader propertyReader)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
            _propertyReader = propertyReader;
        }

        public string ConnectionString { get; set; }

        public void Update<T>(T instance)
        {
            var className = typeof(T).Name;
            var tableName = _convention.GetTableName(className);

            var columnValues = _propertyReader.GetColumnValues(instance);

            var primaryKeyColumn = GetPrimaryKeyColumn<T>();

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, columnValues);

            command.ExecuteNonQuery(ConnectionString);
        }

        public void Update<T>(IDictionary<string, object> constraints, IDictionary<string, object> setters)
        {
            var className = typeof(T).Name;
            var tableName = _convention.GetTableName(className);

            var columnConstraints = constraints.TransformKeys(_convention.GetColumnName);
            var columnSetters = setters.TransformKeys(_convention.GetColumnName);

            var primaryKeyColumn = GetPrimaryKeyColumn<T>();

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, columnConstraints, columnSetters);

            command.ExecuteNonQuery(ConnectionString);
        }

        private string GetPrimaryKeyColumn<T>()
        {
            var idProperty = typeof(T).GetProperties()
                .Select(x => x.Name)
                .First(_convention.IsIdProperty);

            return _convention.GetColumnName(idProperty);
        }
    }
}