using System;
using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class ObjectQuerySpecification
    {
        public ObjectQuerySpecification(Type resultType)
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

        public bool IsPagingQuery
        {
            get { return Page != null && Page.PageSize > 0; }
        }

        public ObjectQueryJoinSpecification JoinSpecification { get; set; }

        public bool HasJoinSpecification
        {
            get { return JoinSpecification != null; }
        }
    }
}