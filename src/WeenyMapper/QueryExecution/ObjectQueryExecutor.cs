using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using WeenyMapper.Conventions;
using WeenyMapper.Extensions;
using WeenyMapper.Mapping;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class ObjectQueryExecutor : IObjectQueryExecutor
    {
        private readonly IConvention _convention;
        private readonly ISqlGenerator _sqlGenerator;
        private readonly IDbCommandExecutor _dbCommandExecutor;
        private readonly IEntityMapper _entityMapper;
        private readonly IConventionDataReader _conventionDataReader;

        public ObjectQueryExecutor(
            IConvention convention, 
            ISqlGenerator sqlGenerator, 
            IDbCommandExecutor dbCommandExecutor, 
            IEntityMapper entityMapper,
            IConventionDataReader conventionDataReader)
        {
            _convention = convention;
            _sqlGenerator = sqlGenerator;
            _dbCommandExecutor = dbCommandExecutor;
            _entityMapper = entityMapper;
            _conventionDataReader = conventionDataReader;
        }

        public string ConnectionString { get; set; }

        public TScalar FindScalar<T, TScalar>(QuerySpecification querySpecification)
        {
            var command = CreateCommand<T>(querySpecification);

            return _dbCommandExecutor.ExecuteScalar<TScalar>(command, ConnectionString);
        }

        public IList<TScalar> FindScalarList<T, TScalar>(QuerySpecification querySpecification)
        {
            var command = CreateCommand<T>(querySpecification);

            return _dbCommandExecutor.ExecuteScalarList<TScalar>(command, ConnectionString);
        }

        public IList<T> Find<T>(QuerySpecification querySpecification) where T : new()
        {
            var command = CreateCommand<T>(querySpecification);

            return ReadEntities<T>(command);
        }

        private DbCommand CreateCommand<T>(QuerySpecification querySpecification)
        {
            if (!querySpecification.PropertiesToSelect.Any())
            {
                querySpecification.PropertiesToSelect.AddRange(GetPropertiesInTargetType<T>());
            }

            var columnNamesToSelect = querySpecification.PropertiesToSelect.Select(_convention.GetColumnName);
            var translatedOrderByStatements = querySpecification.OrderByStatements.Select(x => x.Translate(_convention));
            var tableName = _convention.GetTableName(querySpecification.TableName);

            var sqlQuery = new QuerySpecification
                {
                    ColumnsToSelect = columnNamesToSelect,
                    QueryExpression = querySpecification.QueryExpression.Translate(_convention),
                    TableName = tableName,
                    OrderByStatements = translatedOrderByStatements.ToList(),
                    RowCountLimit =  querySpecification.RowCountLimit,
                    Page = querySpecification.Page
                };

            return _sqlGenerator.GenerateSelectQuery(sqlQuery);
        }

        private IEnumerable<string> GetPropertiesInTargetType<T>()
        {
            return _conventionDataReader.GetColumnNames(typeof(T));
        }

        private IList<T> ReadEntities<T>(DbCommand command) where T : new()
        {
            return _dbCommandExecutor.ExecuteQuery(command, _entityMapper.CreateInstance<T>, ConnectionString);
        }
    }
}