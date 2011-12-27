using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using WeenyMapper.Conventions;
using WeenyMapper.QueryParsing;
using WeenyMapper.SqlGeneration;
using System.Linq;

namespace WeenyMapper
{
    public class DynamicSelectExecutor : DynamicObject
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IQueryParser _queryParser;
        private string _tableName;
        private Dictionary<string, object> _constraints;

        public DynamicSelectExecutor() : this(new DefaultConvention(), new TSqlGenerator(), new QueryParser()) {}

        public DynamicSelectExecutor(IConvention convention, ISqlGenerator sqlGenerator, IQueryParser queryParser)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
            _queryParser = queryParser;
        }

        public string ConnectionString { get; set; }

        public T Execute<T>() where T : new()
        {
            var propertiesInTargetType = typeof(T).GetProperties();
            var columnNamesToSelect = propertiesInTargetType.Select(x => _convention.GetColumnName(x.Name));
            var command = _sqlGenerator.GenerateSelectQuery(_tableName, columnNamesToSelect, _constraints);

            var values = CreateResult(command);

            var instance = new T();
            var instanceType = typeof(T);

            foreach (var value in values)
            {
                var property = instanceType.GetProperty(value.Key);
                property.SetValue(instance, value.Value, null);
            }

            return instance;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var query = _queryParser.ParseSelectQuery(binder.Name);

            _tableName = _convention.GetTableName(query.ClassName);
            var columnName = _convention.GetColumnName(query.ConstraintProperties[0]);
            var columnValue = args[0];

            _constraints = new Dictionary<string, object>();
            _constraints[columnName] = columnValue;

            result = this;

            return true;
        }

        private IDictionary<string, object> CreateResult(DbCommand command)
        {
            IDictionary<string, object> values;

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                command.Connection = connection;

                var reader = command.ExecuteReader();

                reader.Read();

                values = GetValues(reader);

                command.Dispose();
            }

            return values;
        }

        private Dictionary<string, object> GetValues(DbDataReader reader)
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