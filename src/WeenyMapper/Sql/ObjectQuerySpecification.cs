using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class ObjectQuerySpecification<T>
    {
        public ObjectQuerySpecification()
        {
            PropertiesToSelect = new List<string>();
            OrderByStatements = new List<OrderByStatement>();
            QueryExpression = QueryExpression.Create();
        }

        public IList<string> PropertiesToSelect { get; set; }
        public QueryExpression QueryExpression { get; set; }
        public IList<OrderByStatement> OrderByStatements { get; set; }
        public int RowCountLimit { get; set; }
        public Page Page { get; set; }

        public string ResultTypeName
        {
            get { return typeof(T).Name; }
        }

        public bool IsPagingQuery
        {
            get { return Page != null && Page.PageSize > 0; }
        }
    }
}