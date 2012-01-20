using System;
using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class QuerySpecification
    {
        public QuerySpecification()
        {
            ColumnsToSelect = new List<string>();
            OrderByStatements = new List<OrderByStatement>();
            QueryExpression = QueryExpression.Create();
        }

        public string TableName { get; set; }
        public IList<string> ColumnsToSelect { get; set; }
        public QueryExpression QueryExpression { get; set; }
        public IList<OrderByStatement> OrderByStatements { get; set; }
        public int RowCountLimit { get; set; }
        public Page Page { get; set; }
        public string PrimaryKeyColumnName { get; set; }

        public bool IsPagingQuery
        {
            get { return Page != null && Page.PageSize > 0; }
        }

        public static QuerySpecification CreateFor<T>()
        {
            return new QuerySpecification
                {
                    TableName = typeof(T).Name,
                };
        }
    }
}