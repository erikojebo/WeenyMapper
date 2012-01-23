using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using WeenyMapper.Extensions;
using WeenyMapper.Mapping;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectQueryExecutor : IObjectQueryExecutor
    {
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IDbCommandExecutor _dbCommandExecutor;
        private readonly IEntityMapper _entityMapper;
        private readonly IConventionReader _conventionReader;

        public ObjectQueryExecutor(
            ISqlGenerator sqlGenerator,
            IDbCommandExecutor dbCommandExecutor,
            IEntityMapper entityMapper,
            IConventionReader conventionReader)
        {
            _sqlGenerator = sqlGenerator;
            _dbCommandExecutor = dbCommandExecutor;
            _entityMapper = entityMapper;
            _conventionReader = conventionReader;
        }

        public string ConnectionString { get; set; }

        public TScalar FindScalar<T, TScalar>(ObjectQuerySpecification<T> querySpecification)
        {
            var command = CreateCommand<T>(querySpecification);

            return _dbCommandExecutor.ExecuteScalar<TScalar>(command, ConnectionString);
        }

        public IList<TScalar> FindScalarList<T, TScalar>(ObjectQuerySpecification<T> querySpecification)
        {
            var command = CreateCommand<T>(querySpecification);

            return _dbCommandExecutor.ExecuteScalarList<TScalar>(command, ConnectionString);
        }

        public IList<T> Find<T>(ObjectQuerySpecification<T> querySpecification) where T : new()
        {
            var command = CreateCommand<T>(querySpecification);

            return ReadEntities<T>(command);
        }

        private DbCommand CreateCommand<T>(ObjectQuerySpecification<T> querySpecification)
        {
            if (!querySpecification.PropertiesToSelect.Any())
            {
                querySpecification.PropertiesToSelect.AddRange(GetPropertiesInTargetType<T>());
            }

            var columnNamesToSelect = querySpecification.PropertiesToSelect.Select(_conventionReader.GetColumnNamee<T>);
            var translatedOrderByStatements = querySpecification.OrderByStatements.Select(x => x.Translate<T>(_conventionReader));
            var tableName = _conventionReader.GetTableName(querySpecification.ResultType);

            var sqlQuery = new SqlQuerySpecification
                {
                    ColumnsToSelect = columnNamesToSelect.ToList(),
                    QueryExpression = querySpecification.QueryExpression.Translate(_conventionReader),
                    TableName = tableName,
                    OrderByStatements = translatedOrderByStatements.ToList(),
                    RowCountLimit = querySpecification.RowCountLimit,
                    Page = querySpecification.Page
                };

            return _sqlGenerator.GenerateSelectQuery(sqlQuery);
        }

        private IEnumerable<string> GetPropertiesInTargetType<T>()
        {
            return _conventionReader.GetColumnNames(typeof(T));
        }

        private IList<T> ReadEntities<T>(DbCommand command) where T : new()
        {
            return _dbCommandExecutor.ExecuteQuery(command, _entityMapper.CreateInstance<T>, ConnectionString);
        }
    }
}