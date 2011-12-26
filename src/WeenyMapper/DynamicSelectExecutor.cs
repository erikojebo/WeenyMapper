using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text.RegularExpressions;
using WeenyMapper.Conventions;

namespace WeenyMapper
{
    public class DynamicSelectExecutor : DynamicObject
    {
        private readonly IConvention _convention;

        public DynamicSelectExecutor(IConvention convention)
        {
            _convention = convention;
        }

        public string ConnectionString { get; set; }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var regex = new Regex("(?<className>.*)By(?<propertyName>.*)");
            var match = regex.Match(binder.Name);

            var tableName = _convention.GetTableName(match.Groups["className"].Value);
            
            var columnName = _convention.GetColumnName(match.Groups["propertyName"].Value);
            var columnValue = args[0];

            var whereClause = string.Format("{0} = '{1}'", columnName, columnValue);
            var commandString = string.Format("select * from {0} where {1}", tableName, whereClause);

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(commandString, connection))
                {
                    var reader = command.ExecuteReader();

                    var values = new Dictionary<string, object>();

                    reader.Read();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        var value = reader.GetValue(i);

                        values[name] = value;
                    }

                    result = new DynamicQueryResult(values);
                }
            }

            return true;
        }
    }
}