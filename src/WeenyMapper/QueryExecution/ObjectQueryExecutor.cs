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
        private SqlQuery _sqlQuery;

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

        public IList<T> Find<T>(ObjectQuery query, SqlQuery sqlQuery) where T : new()
        {
            _sqlQuery = sqlQuery;

            var command = CreateCommand(query);

            return ReadEntities<T>(command, query);
        }

        public TScalar FindScalar<T, TScalar>(ObjectQuery query, SqlQuery sqlQuery)
        {
            _sqlQuery = sqlQuery;
            var command = CreateCommand(query);

            return _dbCommandExecutor.ExecuteScalar<TScalar>(command, ConnectionString);
        }

        public IList<TScalar> FindScalarList<T, TScalar>(ObjectQuery query, SqlQuery sqlQuery)
        {
            _sqlQuery = sqlQuery;
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
            var sqlQuery = _sqlQuery ?? new SqlQuery(_conventionReader);

            sqlQuery.QueryExpressionTree = query.QueryExpressionTree.Translate(_conventionReader);

            foreach (var objectSubQuery in query.SubQueries)
            {
                AddSqlQuerySpecification(objectSubQuery, sqlQuery);
            }

            foreach (var objectSubQueryJoin in query.Joins)
            {
                AddSqlQueryJoinSpecification(objectSubQueryJoin, sqlQuery);
            }

            return sqlQuery;
        }

        private void AddSqlQuerySpecification(AliasedObjectSubQuery subQuery, SqlQuery sqlQuery)
        {
            var translatedOrderByStatements = subQuery.OrderByStatements.Select(x => x.Translate(_conventionReader, subQuery.ResultType));
            var tableName = _conventionReader.GetTableName(subQuery.ResultType);

            var spec = sqlQuery.GetOrCreateSubQuery(subQuery.Alias, subQuery.ResultType);

            spec.TableName = tableName;
            spec.OrderByStatements = translatedOrderByStatements.ToList();
            spec.PrimaryKeyColumnName = _conventionReader.TryGetPrimaryKeyColumnName(subQuery.ResultType);
            spec.Alias = subQuery.Alias;
        }

        private void AddSqlQueryJoinSpecification(ObjectSubQueryJoin joinSpecification, SqlQuery query)
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

            query.AddJoin(joinSpec, joinSpecification.ChildSubQuery.Alias, joinSpecification.ParentSubQuery.Alias);
        }

        private IList<T> ReadEntities<T>(DbCommand command, ObjectQuery objectQuery) where T : new()
        {
            var resultSet = _dbCommandExecutor.ExecuteQuery(command, ConnectionString);

            var objectRelations = objectQuery.Joins.Select(ObjectRelation.Create).ToList();

            if (objectRelations.Any())
            {
                return _entityMapper.CreateInstanceGraphs<T>(resultSet, objectRelations);
            }

            return _entityMapper.CreateInstanceGraphs<T>(resultSet);
        }
    }
}