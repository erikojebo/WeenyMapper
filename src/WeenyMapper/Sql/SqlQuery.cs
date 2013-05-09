using System.Collections.Generic;
using System.Linq;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class SqlQuery
    {
        public SqlQuery()
        {
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
            if (!string.IsNullOrWhiteSpace(alias))
                return SubQueries.FirstOrDefault(x => x.Alias == alias);

            return SubQueries.FirstOrDefault(x => x.TableName == tableName);
        }

        public IEnumerable<QueryExpressionPart> GetQueryExpressions()
        {
            return SubQueries.SelectMany(x => x.QueryExpressions ).OrderBy(x => x.MetaData.OrderIndex);
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
    }
}