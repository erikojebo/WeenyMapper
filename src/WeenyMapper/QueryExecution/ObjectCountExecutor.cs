using System.Collections.Generic;
using WeenyMapper.Conventions;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectCountExecutor : IObjectCountExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IConventionDataReader _conventionDataReader;
        private readonly IDbCommandExecutor _dbCommandExecutor;
        private readonly IConvention _convention;

        public ObjectCountExecutor(ISqlGenerator sqlGenerator, IConventionDataReader conventionDataReader, IDbCommandExecutor dbCommandExecutor, IConvention convention)
        {
            _sqlGenerator = sqlGenerator;
            _conventionDataReader = conventionDataReader;
            _dbCommandExecutor = dbCommandExecutor;
            _convention = convention;
        }

        public string ConnectionString { get; set; }

        public int Count<T>(QueryExpression queryExpression)
        {
            var tableName = _conventionDataReader.GetTableName<T>();
            var command = _sqlGenerator.CreateCountCommand(tableName, queryExpression.Translate(_convention));

            return _dbCommandExecutor.ExecuteScalar<int>(command, ConnectionString);
        }
    }
}