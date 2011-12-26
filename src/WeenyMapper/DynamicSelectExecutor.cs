using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text.RegularExpressions;
using WeenyMapper.Conventions;
using WeenyMapper.SqlGeneration;

namespace WeenyMapper
{
    public class DynamicSelectExecutor : DynamicObject
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;

        public DynamicSelectExecutor() : this(new DefaultConvention(), new TSqlGenerator()) {}

        public DynamicSelectExecutor(IConvention convention, ISqlGenerator sqlGenerator)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
        }

        public string ConnectionString { get; set; }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var regex = new Regex("(?<className>.*)By(?<propertyName>.*)");
            var match = regex.Match(binder.Name);

            var tableName = _convention.GetTableName(match.Groups["className"].Value);

            var columnName = _convention.GetColumnName(match.Groups["propertyName"].Value);
            var columnValue = args[0];

            var constraints = new Dictionary<string, object>();
            constraints[columnName] = columnValue;

            var command = _sqlGenerator.GenerateSelectQuery(tableName, constraints);

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                command.Connection = connection;

                var reader = command.ExecuteReader();

                reader.Read();

                var values = GetValues(reader);

                result = new DynamicQueryResult(values);

                command.Dispose();
            }

            return true;
        }

        private Dictionary<string, object> GetValues(SqlDataReader reader)
        {
            var values = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.GetValue(i);

                values[name] = value;
            }
            return values;
        }
    }
}