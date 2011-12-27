using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using WeenyMapper.Conventions;
using WeenyMapper.SqlGeneration;

namespace WeenyMapper.QueryExecution
{
    public class ObjectQueryExecutor : IObjectQueryExecutor
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;

        public ObjectQueryExecutor() : this(new DefaultConvention(), new TSqlGenerator()) {}

        public ObjectQueryExecutor(IConvention convention, ISqlGenerator sqlGenerator)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
        }

        public string ConnectionString { get; set; }

        public T Find<T>(string className, IDictionary<string, object> constraints) where T : new()
        {
            var propertiesInTargetType = typeof(T).GetProperties();
            var columnNamesToSelect = propertiesInTargetType.Select(x => _convention.GetColumnName(x.Name));
            var tableName = _convention.GetTableName(className);

            var columnConstraints = GetColumnConstraints(constraints);

            var command = _sqlGenerator.GenerateSelectQuery(tableName, columnNamesToSelect, columnConstraints);

            var values = CreateResult(command);

            var instance = new T();

            foreach (var value in values)
            {
                var property = propertiesInTargetType.First(x => _convention.GetColumnName(x.Name) == value.Key);
                property.SetValue(instance, value.Value, null);
            }

            return instance;
        }

        private IDictionary<string, object> GetColumnConstraints(IDictionary<string, object> constraints)
        {
            var columnConstraints = new Dictionary<string, object>();

            foreach (var constraint in constraints)
            {
                var columnName = _convention.GetColumnName(constraint.Key);
                columnConstraints[columnName] = constraint.Value;
            }

            return columnConstraints;
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