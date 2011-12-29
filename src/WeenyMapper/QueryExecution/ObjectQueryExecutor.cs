using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using WeenyMapper.Conventions;
using WeenyMapper.Sql;
using WeenyMapper.Extensions;

namespace WeenyMapper.QueryExecution
{
    public class ObjectQueryExecutor : IObjectQueryExecutor
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IDbCommandExecutor _dbCommandExecutor;
        private PropertyInfo[] _propertiesInTargetType;

        public ObjectQueryExecutor(IConvention convention, ISqlGenerator sqlGenerator, IDbCommandExecutor dbCommandExecutor)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
            _dbCommandExecutor = dbCommandExecutor;
        }

        public string ConnectionString { get; set; }

        public IList<T> Find<T>(string className, IDictionary<string, object> constraints) where T : new()
        {
            _propertiesInTargetType = typeof(T).GetProperties();

            var columnNamesToSelect = _propertiesInTargetType.Select(x => _convention.GetColumnName(x.Name));
            var tableName = _convention.GetTableName(className);

            var columnConstraints = constraints.TransformKeys(_convention.GetColumnName);

            var command = _sqlGenerator.GenerateSelectQuery(tableName, columnNamesToSelect, columnConstraints);

            return ReadEntities<T>(command);
        }

        private T CreateInstance<T>(DbDataReader dataReader) where T : new()
        {
            var values = GetValues(dataReader);

            return CreateInstance<T>(values);
        }

        private IList<T> ReadEntities<T>(DbCommand command) where T : new()
        {
            return _dbCommandExecutor.ExecuteQuery(command, CreateInstance<T>, ConnectionString);
        }

        private T CreateInstance<T>(IDictionary<string, object> dictionary) where T : new()
        {
            var instance = new T();

            foreach (var value in dictionary)
            {
                var property = _propertiesInTargetType.First(x => _convention.GetColumnName(x.Name) == value.Key);
                property.SetValue(instance, value.Value, null);
            }

            return instance;
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