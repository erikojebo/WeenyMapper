using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectCountExecutor : IObjectCountExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IConventionReader _conventionReader;
        private readonly IDbCommandExecutor _dbCommandExecutor;

        public ObjectCountExecutor(ISqlGenerator sqlGenerator, IConventionReader conventionReader, IDbCommandExecutor dbCommandExecutor)
        {
            _sqlGenerator = sqlGenerator;
            _conventionReader = conventionReader;
            _dbCommandExecutor = dbCommandExecutor;
        }

        public string ConnectionString { get; set; }

        public int Count<T>(QueryExpression queryExpression)
        {
            var tableName = _conventionReader.GetTableName<T>();
            var command = _sqlGenerator.CreateCountCommand(tableName, queryExpression.Translate(_conventionReader));

            return _dbCommandExecutor.ExecuteScalar<int>(command, ConnectionString);
        }
    }
}