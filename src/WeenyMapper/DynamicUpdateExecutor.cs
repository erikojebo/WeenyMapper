using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using WeenyMapper.Conventions;
using WeenyMapper.SqlGeneration;

namespace WeenyMapper
{
    public class DynamicUpdateExecutor : DynamicObject
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;

        public DynamicUpdateExecutor() : this(new DefaultConvention(), new TSqlGenerator()) {}

        public DynamicUpdateExecutor(IConvention convention, ISqlGenerator sqlGenerator)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
        }

        public string ConnectionString { get; set; }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var className = binder.Name;
            var tableName = _convention.GetTableName(className);

            var objectToInsert = args[0];

            var propertyValues = GetPropertyValues(objectToInsert);

            var primaryKeyColumn = propertyValues.Keys.First(_convention.IsIdProperty);

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, propertyValues);

            ExecuteCommand(command);

            result = null;

            return true;
        }

        private void ExecuteCommand(DbCommand command)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                command.Connection = connection;

                command.ExecuteNonQuery();

                connection.Dispose();
            }
        }

        private Dictionary<string, object> GetPropertyValues(object objectToInsert)
        {
            var properties = objectToInsert.GetType().GetProperties();

            var propertyValues = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                propertyValues[property.Name] = property.GetValue(objectToInsert, null);
            }
            return propertyValues;
        }
    }
}