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

            var idProperty = instance.GetType().GetProperties()
                .Select(x => x.Name)
                .First(_convention.IsIdProperty);

            var primaryKeyColumn = _convention.GetColumnName(idProperty);

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, columnValues);

            command.ExecuteNonQuery(ConnectionString);
        }
    }
}