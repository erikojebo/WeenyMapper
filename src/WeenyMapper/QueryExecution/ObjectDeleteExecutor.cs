using System;
using WeenyMapper.Async;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectDeleteExecutor : IObjectDeleteExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IConventionReader _conventionReader;
        private readonly IDbCommandExecutor _dbCommandExecutor;

        public ObjectDeleteExecutor(ISqlGenerator sqlGenerator, IConventionReader conventionReader, IDbCommandExecutor dbCommandExecutor)
        {
            _sqlGenerator = sqlGenerator;
            _conventionReader = conventionReader;
            _dbCommandExecutor = dbCommandExecutor;
        }

        public string ConnectionString { get; set; }

        public void Delete<T>(T instance)
        {
            var tableName = _conventionReader.GetTableName<T>();

            var primaryKeyColumnName = _conventionReader.GetPrimaryKeyColumnName<T>();
            var primaryKeyValue = _conventionReader.GetPrimaryKeyValue(instance);

            var constraintExpression = QueryExpression.Create(new EqualsExpression(primaryKeyColumnName, primaryKeyValue));

            var command = _sqlGenerator.CreateDeleteCommand(tableName, constraintExpression);

            _dbCommandExecutor.ExecuteNonQuery(command, ConnectionString);
        }

        public void DeleteAsync<T>(T entity, Action callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(() => Delete(entity), callback, errorCallback);
        }

        public int Delete<T>(QueryExpression queryExpression)
        {
            var tableName = _conventionReader.GetTableName<T>();
            var command = _sqlGenerator.CreateDeleteCommand(tableName, queryExpression.Translate(_conventionReader));

            return _dbCommandExecutor.ExecuteNonQuery(command, ConnectionString);
        }
    }
}