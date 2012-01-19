using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class QuerySpecification
    {
        public QuerySpecification()
        {
            PropertiesToSelect = new List<string>();
            OrderByStatements = new List<OrderByStatement>();
        }

        public string TableName { get; set; }
        public IEnumerable<string> ColumnsToSelect { get; set; }
        public QueryExpression QueryExpression { get; set; }
        public IList<string> PropertiesToSelect { get; set; }
        public IList<OrderByStatement> OrderByStatements { get; set; }
        public int RowCountLimit { get; set; }

        public static QuerySpecification CreateFor<T>()
        {
            return new QuerySpecification
                {
                    TableName = typeof(T).Name,
                    QueryExpression = QueryExpression.Create()
                };
        }
    }
}