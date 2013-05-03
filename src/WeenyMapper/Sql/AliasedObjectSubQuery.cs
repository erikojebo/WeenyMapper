using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class AliasedObjectSubQuery
    {
        public AliasedObjectSubQuery(Type resultType)
        {
            PropertiesToSelect = new List<string>();
            OrderByStatements = new List<OrderByStatement>();
            QueryExpression = QueryExpression.Create();
            ResultType = resultType;
        }

        public IList<string> PropertiesToSelect { get; set; }
        public QueryExpression QueryExpression { get; set; }
        public IList<OrderByStatement> OrderByStatements { get; set; }
        public int RowCountLimit { get; set; }
        public Page Page { get; set; }
        public Type ResultType { get; set; }
        public string Alias { get; set; }

        public bool IsPagingQuery
        {
            get { return Page != null && Page.PageSize > 0; }
        }

        public void AddConjunctionExpression(QueryExpression queryExpression)
        {
            if (Equals(QueryExpression, QueryExpression.Create()))
                QueryExpression = queryExpression;
            else
                QueryExpression = new AndExpression(QueryExpression, queryExpression);

        }

        public void AddDisjunctionExpression(QueryExpression queryExpression)
        {
            if (Equals(QueryExpression, QueryExpression.Create()))
                QueryExpression = queryExpression;
            else
                QueryExpression = new OrExpression(QueryExpression, queryExpression);
        }
    }
}