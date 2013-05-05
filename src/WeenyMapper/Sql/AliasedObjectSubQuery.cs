using System;
using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class AliasedObjectSubQuery
    {
        public AliasedObjectSubQuery(Type resultType)
        {
            PropertiesToSelect = new List<string>();
            OrderByStatements = new List<OrderByStatement>();
            QueryExpressions = new List<QueryExpressionPart>();
            ResultType = resultType;
        }

        public IList<string> PropertiesToSelect { get; set; }
        public QueryExpression QueryExpression { get; set; }
        public QueryExpressionMetaData QueryExpressionMetaData { get; set; }
        public List<QueryExpressionPart> QueryExpressions { get; set; }
        public IList<OrderByStatement> OrderByStatements { get; set; }
        public int RowCountLimit { get; set; }
        public Page Page { get; set; }
        public Type ResultType { get; set; }
        public string Alias { get; set; }

        public bool IsPagingQuery
        {
            get { return Page != null && Page.PageSize > 0; }
        }

        public void AddQueryExpression(QueryExpression queryExpression, QueryExpressionMetaData metaData)
        {
            QueryExpressions.Add(new QueryExpressionPart(queryExpression, metaData));
        }
    }
}