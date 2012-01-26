using System;
using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class SqlQuerySpecification
    {
        public SqlQuerySpecification()
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

        public SqlQueryJoinSpecification JoinSpecification { get; set; }

        public bool HasJoinSpecification
        {
            get { return JoinSpecification != null; }
        }

        public static SqlQuerySpecification CreateFor<T>()
        {
            return new SqlQuerySpecification
                {
                    TableName = typeof(T).Name,
                };
        }
    }
}