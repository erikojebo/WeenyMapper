using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Conventions;
using WeenyMapper.QueryParsing;
using WeenyMapper.Reflection;
using WeenyMapper.Extensions;

namespace WeenyMapper.Sql
{
    public class SqlQuery
    {
        private readonly IConventionReader _conventionReader;

        public SqlQuery() : this(new ConventionReader(new DefaultConvention()))
        {
        }

        public SqlQuery(IConventionReader conventionReader)
        {
            _conventionReader = conventionReader;

            SubQueries = new List<AliasedSqlSubQuery>();
            Joins = new List<SqlSubQueryJoin>();
            QueryExpressionTree = new EmptyQueryExpressionTree();
        }

        public List<AliasedSqlSubQuery> SubQueries { get; set; }
        public List<SqlSubQueryJoin> Joins { get; set; }
        public QueryExpressionTree QueryExpressionTree { get; set; }

        public bool IsJoinQuery
        {
            get { return Joins.Any(); }
        }

        public IEnumerable<OrderByStatement> OrderByStatements
        {
            get { return SubQueries.SelectMany(x => x.OrderByStatements).OrderBy(x => x.OrderIndex); }
        }

        public void AddJoin(SqlSubQueryJoin joinSpec, string childAlias, string parentAlias)
        {
            joinSpec.ParentSubQuery = GetSubQuery(joinSpec.ParentTableName, parentAlias);
            joinSpec.ChildSubQuery = GetSubQuery(joinSpec.ChildTableName, childAlias);

            Joins.Add(joinSpec);
        }

        private AliasedSqlSubQuery GetSubQuery(string tableName, string alias)
        {
            return SubQueries.FirstOrDefault(x => x.TableName == tableName && x.Alias == alias);
        }

        public void AddConjunctionExpression(string tableIdentifier, QueryExpression queryExpression)
        {
            var leaf = new TranslatedQueryExpressionTreeLeaf(queryExpression, tableIdentifier);
            QueryExpressionTree = QueryExpressionTree.And(leaf);
        }

        public void AddDisjunctionExpression(string tableIdentifier, QueryExpression queryExpression)
        {
            var leaf = new TranslatedQueryExpressionTreeLeaf(queryExpression, tableIdentifier);
            QueryExpressionTree = QueryExpressionTree.Or(leaf);
        }

        public static SqlQuery Create<T>(IConventionReader conventionReader)
        {
            var sqlQuery = new SqlQuery(conventionReader);
         
            sqlQuery.EnsureSubQuery<T>();
            
            return sqlQuery;
        }

        public void EnsureSubQuery<T>(string alias=null)
        {
            EnsureSubQuery(alias, typeof(T));
        }

        public void EnsureSubQuery(string alias, Type type)
        {
            var tableName = _conventionReader.GetTableName(type);

            if (!HasSubQuery(tableName, alias))
                CreateSubQuery(alias, type);
        }

        private bool HasSubQuery(string tableName, string alias = null)
        {
            return SubQueries.Any(x => x.TableName == tableName && x.Alias == alias);
        }

        private AliasedSqlSubQuery GetOrCreateSubQuery<T>(string alias)
        {
            return GetOrCreateSubQuery(alias, typeof(T));
        }

        public AliasedSqlSubQuery GetOrCreateSubQuery(string alias, Type type)
        {
            EnsureSubQuery(alias, type);
            return GetSubQuery(_conventionReader.GetTableName(type), alias);
        }

        private void CreateSubQuery<T>(string alias)
        {
            CreateSubQuery(alias, typeof(T));
        }

        private void CreateSubQuery(string alias, Type type)
        {
            var subQuery = AliasedSqlSubQuery.Create(alias, type, _conventionReader);

            SubQueries.Add(subQuery);
        }

        public void AddPropertiesToSelect<T>(string alias, IEnumerable<string> propertyNames)
        {
            var subQuery = GetOrCreateSubQuery<T>(alias);
            
            var columnsToSelect = propertyNames.Select(x => _conventionReader.GetColumnName<T>(x)).ToList();

            subQuery.ExplicitlySpecifiedColumnsToSelect.AddRange(columnsToSelect);
        }

        public IEnumerable<string> GetColumnNamesToSelect(string alias, Type type)
        {
            var subQuery = GetOrCreateSubQuery(alias, type);
            return GetColumnNamesToSelect(subQuery);
        }

        public IEnumerable<string> GetColumnNamesToSelect(AliasedSqlSubQuery subQuery)
        {
            var hasExplicitColumnsToSelect = SubQueries.Any(x => x.HasExplicitlySpecifiedColumnsToSelect);

            if (hasExplicitColumnsToSelect)
                return subQuery.ExplicitlySpecifiedColumnsToSelect;

            return subQuery.AllSelectableColumnNames;
        }
    }
}