using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using WeenyMapper.Conventions;
using WeenyMapper.SqlGeneration;

namespace WeenyMapper.QueryBuilding
{
    public class DynamicUpdateBuilder : DynamicObject
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;

        public DynamicUpdateBuilder() : this(new DefaultConvention(), new TSqlGenerator()) {}

        public DynamicUpdateBuilder(IConvention convention, ISqlGenerator sqlGenerator)
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
            var columnValues = GetColumnValues(propertyValues);
            var idProperty = propertyValues.Keys.First(_convention.IsIdProperty);
            var primaryKeyColumn = _convention.GetColumnName(idProperty);
            
            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, columnValues);

            ExecuteCommand(command);

            result = null;

            return true;
        }

        private Dictionary<string, object > GetColumnValues(Dictionary<string, object> propertyValues)
        {
            var columnConstraints = new Dictionary<string, object>();

            foreach (var propertyValue in propertyValues)
            {
                var columnName = _convention.GetColumnName(propertyValue.Key);
                columnConstraints[columnName] = propertyValue.Value;
            }

            return columnConstraints;

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