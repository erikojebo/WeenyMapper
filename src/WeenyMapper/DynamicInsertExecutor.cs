using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;

namespace WeenyMapper
{
    public class DynamicInsertExecutor : DynamicObject
    {
        private readonly string _connectionString;

        public DynamicInsertExecutor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var className = binder.Name;
            var tableName = "[" + className + "]"; // Convention logic will be used here at a later point

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
            using (var connection = new SqlConnection(_connectionString))
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