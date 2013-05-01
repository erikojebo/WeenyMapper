using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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

        public TScalar FindScalar<T, TScalar>(AliasedObjectSubQuery subQuery)
        {
            var command = CreateCommand(subQuery);

            return _dbCommandExecutor.ExecuteScalar<TScalar>(command, ConnectionString);
        }

        public IList<TScalar> FindScalarList<T, TScalar>(AliasedObjectSubQuery subQuery)
        {
            var command = CreateCommand(subQuery);

            return _dbCommandExecutor.ExecuteScalarList<TScalar>(command, ConnectionString);
        }

        public IList<T> Find<T>(ObjectQuery query) where T : new()
        {
            var subQuery = query.SubQueries.FirstOrDefault();
            var command = CreateCommand(subQuery);

            return ReadEntities<T>(command, subQuery);
        }

        private DbCommand CreateCommand(AliasedObjectSubQuery subQuery)
        {
            var sqlQuerySpecification = CreateSqlQuerySpecification(subQuery);

            return _sqlGenerator.GenerateSelectQuery(sqlQuerySpecification);
        }

        private SqlQuerySpecification CreateSqlQuerySpecification(AliasedObjectSubQuery subQuery)
        {
            var resultType = subQuery.ResultType;

            var columnNamesToSelect = subQuery.PropertiesToSelect.Select(x => _conventionReader.GetColumnName(x, resultType));

            if (!subQuery.PropertiesToSelect.Any())
            {
                columnNamesToSelect = _conventionReader.GetSelectableColumNames(resultType);
            }

            var translatedOrderByStatements = subQuery.OrderByStatements.Select(x => x.Translate(_conventionReader, resultType));
            var tableName = _conventionReader.GetTableName(resultType);

            var spec = new SqlQuerySpecification
                {
                    ColumnsToSelect = columnNamesToSelect.ToList(),
                    QueryExpression = subQuery.QueryExpression.Translate(_conventionReader),
                    TableName = tableName,
                    OrderByStatements = translatedOrderByStatements.ToList(),
                    RowCountLimit = subQuery.RowCountLimit,
                    Page = subQuery.Page,
                    PrimaryKeyColumnName = _conventionReader.TryGetPrimaryKeyColumnName(subQuery.ResultType)
                };

            if (subQuery.HasJoinSpecification)
            {
                spec.JoinSpecification = CreateSqlQueryJoinSpecification(subQuery.JoinSpecification);
            }

            return spec;
        }

        private SqlQueryJoinSpecification CreateSqlQueryJoinSpecification(ObjectSubQueryJoin @join)
        {
            string manyToOneForeignKeyColumnName;

            if (@join.HasChildProperty)
                manyToOneForeignKeyColumnName = _conventionReader.GetManyToOneForeignKeyColumnName(@join.ChildProperty);
            else
                manyToOneForeignKeyColumnName = _conventionReader.GetColumnName(@join.ChildToParentForeignKeyProperty);

            return new SqlQueryJoinSpecification
                {
                    ChildTableName = _conventionReader.GetTableName(@join.ChildType),
                    ParentTableName = _conventionReader.GetTableName(@join.ParentType),
                    ChildForeignKeyColumnName = manyToOneForeignKeyColumnName,
                    ParentPrimaryKeyColumnName = _conventionReader.GetPrimaryKeyColumnName(@join.ParentType),
                    SqlQuerySpecification = CreateSqlQuerySpecification(@join.AliasedObjectSubQuery)
                };
        }

        private IList<T> ReadEntities<T>(DbCommand command, AliasedObjectSubQuery subQuery) where T : new()
        {
            var resultSet = _dbCommandExecutor.ExecuteQuery(command, ConnectionString);

            var objectRelations = GetObjectRelations(subQuery);

            if (objectRelations.Any())
            {
                return _entityMapper.CreateInstanceGraphs<T>(resultSet, objectRelations);
            }

            return _entityMapper.CreateInstanceGraphs<T>(resultSet);
        }

        private IEnumerable<ObjectRelation> GetObjectRelations(AliasedObjectSubQuery subQuery)
        {
            var objectRelations = new List<ObjectRelation>();
            var currentQuerySpecification = subQuery;

            while (currentQuerySpecification.HasJoinSpecification)
            {
                var primaryType = currentQuerySpecification.ResultType;

                var objectRelation = ObjectRelation.Create(currentQuerySpecification.JoinSpecification, primaryType);

                objectRelations.Add(objectRelation);

                currentQuerySpecification = currentQuerySpecification.JoinSpecification.AliasedObjectSubQuery;
            }
            return objectRelations;
        }
    }
}