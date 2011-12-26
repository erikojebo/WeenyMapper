using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using WeenyMapper.Conventions;

namespace WeenyMapper
{
    public class DynamicInsertExecutor : DynamicObject
    {
        private readonly IConvention _convention;

        public DynamicInsertExecutor() : this(new DefaultConvention()) {}

        public DynamicInsertExecutor(IConvention convention)
        {
            _convention = convention;
        }

        public string ConnectionString { get; set; }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var className = binder.Name;
            var tableName = _convention.GetTableName(className);

            var objectToInsert = args[0];

            var propertyValues = GetPropertyValues(objectToInsert);

            var commandString = CreateCommandString(propertyValues, tableName);

            ExecuteCommand(commandString);

            result = null;

            return true;
        }

        private string CreateCommandString(Dictionary<string, object> propertyValues, string tableName)
        {
            // SQL injection alert here, but simplest possible to get the first acceptance test passing

            var columnNames = string.Join(", ", propertyValues.Keys);
            var quotedValues = propertyValues.Values.Select(x => "'" + x + "'");
            var columnValues = string.Join(", ", quotedValues);

            return string.Format("insert into {0} ({1}) values ({2})", tableName, columnNames, columnValues);
        }

        private void ExecuteCommand(string commandString)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(commandString, connection))
                {
                    command.ExecuteNonQuery();
                }
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