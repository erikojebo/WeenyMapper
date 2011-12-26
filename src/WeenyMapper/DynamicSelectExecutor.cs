using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Text.RegularExpressions;
using WeenyMapper.Conventions;
using WeenyMapper.QueryParsing;
using WeenyMapper.SqlGeneration;

namespace WeenyMapper
{
    public class DynamicSelectExecutor : DynamicObject
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IQueryParser _queryParser;

        public DynamicSelectExecutor() : this(new DefaultConvention(), new TSqlGenerator(), new QueryParser()) {}

        public DynamicSelectExecutor(IConvention convention, ISqlGenerator sqlGenerator, IQueryParser queryParser)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
            _queryParser = queryParser;
        }

        public string ConnectionString { get; set; }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var query = _queryParser.ParseSelectQuery(binder.Name);
            
            var tableName = _convention.GetTableName(query.ClassName);
            var columnName = _convention.GetColumnName(query.ConstraintProperties[0]);
            var columnValue = args[0];

            var constraints = new Dictionary<string, object>();
            constraints[columnName] = columnValue;

            var command = _sqlGenerator.GenerateSelectQuery(tableName, constraints);

            result = CreateResult(command);

            return true;
        }

        private DynamicQueryResult CreateResult(SqlCommand command)
        {
            DynamicQueryResult result;

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

            return result;
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