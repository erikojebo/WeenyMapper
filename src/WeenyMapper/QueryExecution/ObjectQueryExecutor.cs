using System;
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

        public IList<T> Find<T>(ObjectQuery query) where T : new()
        {
            var subQuery = query.SubQueries.FirstOrDefault();
            var command = CreateCommand(query);

            return ReadEntities<T>(command, subQuery);
        }

        public TScalar FindScalar<T, TScalar>(ObjectQuery query)
        {
            var command = CreateCommand(query);

            return _dbCommandExecutor.ExecuteScalar<TScalar>(command, ConnectionString);
        }

        public IList<TScalar> FindScalarList<T, TScalar>(ObjectQuery query)
        {
            var command = CreateCommand(query);

            return _dbCommandExecutor.ExecuteScalarList<TScalar>(command, ConnectionString);
        }

        private DbCommand CreateCommand(ObjectQuery query)
        {
            var sqlQuery = CreateSqlQuery(query);

            return _sqlGenerator.GenerateSelectQuery(sqlQuery);
        }

        private SqlQuery CreateSqlQuery(ObjectQuery query)
        {
            var sqlQuery = new SqlQuery();

            AddSqlQuerySpecification(query.SubQueries.First(), sqlQuery);

            return sqlQuery;
        }

        private void AddSqlQuerySpecification(AliasedObjectSubQuery subQuery, SqlQuery sqlQuery)
        {
            var resultType = subQuery.ResultType;

            var columnNamesToSelect = subQuery.PropertiesToSelect.Select(x => _conventionReader.GetColumnName(x, resultType));

            if (!subQuery.PropertiesToSelect.Any())
            {
                columnNamesToSelect = _conventionReader.GetSelectableColumNames(resultType);
            }

            var translatedOrderByStatements = subQuery.OrderByStatements.Select(x => x.Translate(_conventionReader, resultType));
            var tableName = _conventionReader.GetTableName(resultType);

            var spec = new AliasedSqlSubQuery
                {
                    ColumnsToSelect = columnNamesToSelect.ToList(),
                    QueryExpression = subQuery.QueryExpression.Translate(_conventionReader),
                    TableName = tableName,
                    OrderByStatements = translatedOrderByStatements.ToList(),
                    RowCountLimit = subQuery.RowCountLimit,
                    Page = subQuery.Page,
                    PrimaryKeyColumnName = _conventionReader.TryGetPrimaryKeyColumnName(subQuery.ResultType)
                };

            sqlQuery.SubQueries.Add(spec);

            if (subQuery.HasJoinSpecification)
            {
                var joinSpecification = CreateSqlQueryJoinSpecification(subQuery.JoinSpecification, sqlQuery);

                spec.JoinSpecification = joinSpecification;

                sqlQuery.Joins.Add(joinSpecification);
            }

            return;
        }

        private SqlSubQueryJoin CreateSqlQueryJoinSpecification(ObjectSubQueryJoin joinSpecification, SqlQuery query)
        {
            string manyToOneForeignKeyColumnName;

            if (joinSpecification.HasChildProperty)
                manyToOneForeignKeyColumnName = _conventionReader.GetManyToOneForeignKeyColumnName(joinSpecification.ChildProperty);
            else
                manyToOneForeignKeyColumnName = _conventionReader.GetColumnName(joinSpecification.ChildToParentForeignKeyProperty);

            var joinSpec = new SqlSubQueryJoin
                {
                    ChildTableName = _conventionReader.GetTableName(joinSpecification.ChildType),
                    ParentTableName = _conventionReader.GetTableName(joinSpecification.ParentType),
                    ChildForeignKeyColumnName = manyToOneForeignKeyColumnName,
                    ParentPrimaryKeyColumnName = _conventionReader.GetPrimaryKeyColumnName(joinSpecification.ParentType),
                };

            AddSqlQuerySpecification(joinSpecification.AliasedObjectSubQuery, query);

            return joinSpec;
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