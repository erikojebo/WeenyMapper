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

        public TScalar FindScalar<T, TScalar>(ObjectQuerySpecification querySpecification)
        {
            var command = CreateCommand(querySpecification);

            return _dbCommandExecutor.ExecuteScalar<TScalar>(command, ConnectionString);
        }

        public IList<TScalar> FindScalarList<T, TScalar>(ObjectQuerySpecification querySpecification)
        {
            var command = CreateCommand(querySpecification);

            return _dbCommandExecutor.ExecuteScalarList<TScalar>(command, ConnectionString);
        }

        public IList<T> Find<T>(ObjectQuerySpecification querySpecification) where T : new()
        {
            var command = CreateCommand(querySpecification);

            return ReadEntities<T>(command, querySpecification);
        }

        private DbCommand CreateCommand(ObjectQuerySpecification querySpecification)
        {
            var sqlQuerySpecification = CreateSqlQuerySpecification(querySpecification);

            return _sqlGenerator.GenerateSelectQuery(sqlQuerySpecification);
        }

        private SqlQuerySpecification CreateSqlQuerySpecification(ObjectQuerySpecification querySpecification)
        {
            var resultType = querySpecification.ResultType;

            var columnNamesToSelect = querySpecification.PropertiesToSelect.Select(x => _conventionReader.GetColumnName(x, resultType));

            if (!querySpecification.PropertiesToSelect.Any())
            {
                columnNamesToSelect = _conventionReader.GetSelectableColumNames(resultType);
            }

            var translatedOrderByStatements = querySpecification.OrderByStatements.Select(x => x.Translate(_conventionReader, resultType));
            var tableName = _conventionReader.GetTableName(resultType);

            var spec = new SqlQuerySpecification
                {
                    ColumnsToSelect = columnNamesToSelect.ToList(),
                    QueryExpression = querySpecification.QueryExpression.Translate(_conventionReader),
                    TableName = tableName,
                    OrderByStatements = translatedOrderByStatements.ToList(),
                    RowCountLimit = querySpecification.RowCountLimit,
                    Page = querySpecification.Page,
                    PrimaryKeyColumnName = _conventionReader.TryGetPrimaryKeyColumnName(querySpecification.ResultType)
                };

            if (querySpecification.HasJoinSpecification)
            {
                spec.JoinSpecification = CreateSqlQueryJoinSpecification(querySpecification.JoinSpecification);
            }

            return spec;
        }

        private SqlQueryJoinSpecification CreateSqlQueryJoinSpecification(ObjectQueryJoinSpecification joinSpecification)
        {
            string manyToOneForeignKeyColumnName;

            if (joinSpecification.HasChildProperty)
                manyToOneForeignKeyColumnName = _conventionReader.GetManyToOneForeignKeyColumnName(joinSpecification.ChildProperty);
            else
                manyToOneForeignKeyColumnName = _conventionReader.GetColumnName(joinSpecification.ChildToParentForeignKeyProperty);

            return new SqlQueryJoinSpecification
                {
                    ChildTableName = _conventionReader.GetTableName(joinSpecification.ChildType),
                    ParentTableName = _conventionReader.GetTableName(joinSpecification.ParentType),
                    ChildForeignKeyColumnName = manyToOneForeignKeyColumnName,
                    ParentPrimaryKeyColumnName = _conventionReader.GetPrimaryKeyColumnName(joinSpecification.ParentType),
                    SqlQuerySpecification = CreateSqlQuerySpecification(joinSpecification.ObjectQuerySpecification)
                };
        }

        private IList<T> ReadEntities<T>(DbCommand command, ObjectQuerySpecification querySpecification) where T : new()
        {
            var resultSet = _dbCommandExecutor.ExecuteQuery(command, ConnectionString);

            var objectRelations = GetObjectRelations(querySpecification);

            if (objectRelations.Any())
            {
                return _entityMapper.CreateInstanceGraphs<T>(resultSet, objectRelations);
            }

            return _entityMapper.CreateInstanceGraphs<T>(resultSet);
        }

        private IEnumerable<ObjectRelation> GetObjectRelations(ObjectQuerySpecification querySpecification)
        {
            var objectRelations = new List<ObjectRelation>();
            var currentQuerySpecification = querySpecification;

            while (currentQuerySpecification.HasJoinSpecification)
            {
                var primaryType = currentQuerySpecification.ResultType;

                var objectRelation = ObjectRelation.Create(currentQuerySpecification.JoinSpecification, primaryType);

                objectRelations.Add(objectRelation);

                currentQuerySpecification = currentQuerySpecification.JoinSpecification.ObjectQuerySpecification;
            }
            return objectRelations;
        }
    }
}