using System;
using System.Collections.Generic;
using WeenyMapper.QueryParsing;
using System.Linq;

namespace WeenyMapper.Sql
{
    public class AliasedSqlSubQuery
    {
        public AliasedSqlSubQuery()
        {
            ColumnsToSelect = new List<string>();
            OrderByStatements = new List<OrderByStatement>();
            QueryExpressions = new List<QueryExpressionPart>();
        }

        public string TableName { get; set; }
        public IList<string> ColumnsToSelect { get; set; }
        public List<QueryExpressionPart> QueryExpressions { get; set; }
        public QueryExpressionMetaData QueryExpressionMetaData { get; set; }
        public IList<OrderByStatement> OrderByStatements { get; set; }
        public int RowCountLimit { get; set; }
        public Page Page { get; set; }
        public string PrimaryKeyColumnName { get; set; }
        public string Alias { get; set; }

        public bool IsPagingQuery
        {
            get { return Page != null && Page.PageSize > 0; }
        }

        public bool HasCustomAlias
        {
            get { return !string.IsNullOrWhiteSpace(Alias); }
        }

        public string TableIdentifier
        {
            get { return Alias ?? TableName; }
        }

        public bool HasQuery
        {
            get { return QueryExpressions.Any(); }
        }

        public static AliasedSqlSubQuery CreateFor<T>()
        {
            return new AliasedSqlSubQuery
                {
                    TableName = typeof(T).Name,
                };
        }

        public void AddQueryExpression(QueryExpression queryExpression)
        {
            AddQueryExpression(queryExpression, new QueryExpressionMetaData());
        }

        public void AddQueryExpression(QueryExpression queryExpression, QueryExpressionMetaData metaData)
        {
            QueryExpressions.Add(new QueryExpressionPart(queryExpression, metaData));
        }
    }
}