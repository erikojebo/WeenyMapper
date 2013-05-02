using System;
using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class AliasedSqlSubQuery
    {
        public AliasedSqlSubQuery()
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
        public string Alias { get; set; }

        public bool IsPagingQuery
        {
            get { return Page != null && Page.PageSize > 0; }
        }

        public static AliasedSqlSubQuery CreateFor<T>()
        {
            return new AliasedSqlSubQuery
                {
                    TableName = typeof(T).Name,
                };
        }
    }
}