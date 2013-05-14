using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using WeenyMapper.Mapping;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectQueryExecutor : IObjectQueryExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IDbCommandExecutor _dbCommandExecutor;
        private readonly IEntityMapper _entityMapper;

        public ObjectQueryExecutor(
            ISqlGenerator sqlGenerator,
            IDbCommandExecutor dbCommandExecutor,
            IEntityMapper entityMapper)
        {
            _sqlGenerator = sqlGenerator;
            _dbCommandExecutor = dbCommandExecutor;
            _entityMapper = entityMapper;
        }

        public string ConnectionString { get; set; }

        public IList<T> Find<T>(SqlQuery sqlQuery) where T : new()
        {
            var command = CreateCommand(sqlQuery);

            return ReadEntities<T>(command, sqlQuery);
        }

        public TScalar FindScalar<T, TScalar>(SqlQuery sqlQuery)
        {
            var command = CreateCommand(sqlQuery);

            return _dbCommandExecutor.ExecuteScalar<TScalar>(command, ConnectionString);
        }

        public IList<TScalar> FindScalarList<T, TScalar>(SqlQuery sqlQuery)
        {
            var command = CreateCommand(sqlQuery);

            return _dbCommandExecutor.ExecuteScalarList<TScalar>(command, ConnectionString);
        }

        private DbCommand CreateCommand(SqlQuery sqlQuery)
        {
            return _sqlGenerator.GenerateSelectQuery(sqlQuery);
        }

        private IList<T> ReadEntities<T>(DbCommand command, SqlQuery sqlQuery) where T : new()
        {
            var resultSet = _dbCommandExecutor.ExecuteQuery(command, ConnectionString);

            if (sqlQuery.ObjectRelations.Any())
            {
                return _entityMapper.CreateInstanceGraphs<T>(resultSet, sqlQuery.ObjectRelations);
            }

            return _entityMapper.CreateInstanceGraphs<T>(resultSet);
        }
    }
}