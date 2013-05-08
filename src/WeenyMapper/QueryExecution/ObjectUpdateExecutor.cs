using System.Collections.Generic;
using System.Reflection;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectUpdateExecutor : IObjectUpdateExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IConventionReader _conventionReader;
        private readonly IDbCommandExecutor _dbCommandExecutor;

        public ObjectUpdateExecutor(ISqlGenerator sqlGenerator, IConventionReader conventionReader, IDbCommandExecutor dbCommandExecutor)
        {
            _sqlGenerator = sqlGenerator;
            _conventionReader = conventionReader;
            _dbCommandExecutor = dbCommandExecutor;
        }

        public string ConnectionString { get; set; }

        public void Update<T>(T instance)
        {
            var tableName = _conventionReader.GetTableName<T>();
            var columnValues = _conventionReader.GetColumnValuesForInsertOrUpdate(instance);

            var primaryKeyColumn = _conventionReader.GetPrimaryKeyColumnName<T>();
            var primaryKeyValue = _conventionReader.GetPrimaryKeyValue(instance);

            var constraintExpression = QueryExpression.Create(new EqualsExpression(primaryKeyColumn, primaryKeyValue));

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, constraintExpression, columnValues);

            _dbCommandExecutor.ExecuteNonQuery(command, ConnectionString);
        }

        public int Update<T>(QueryExpression queryExpression, IDictionary<PropertyInfo, object> setters)
        {
            var tableName = _conventionReader.GetTableName<T>();
            var columnSetters = _conventionReader.GetColumnValues<T>(setters);
            var primaryKeyColumn = _conventionReader.GetPrimaryKeyColumnName<T>();

            var command = _sqlGenerator.CreateUpdateCommand(tableName, primaryKeyColumn, queryExpression.Translate(_conventionReader), columnSetters);

            return _dbCommandExecutor.ExecuteNonQuery(command, ConnectionString);
        }
    }
}