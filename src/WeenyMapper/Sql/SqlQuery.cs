using System.Collections.Generic;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.Sql
{
    public class SqlQuery
    {
        public string TableName { get; set; }
        public IEnumerable<string> ColumnsToSelect { get; set; }
        public QueryExpression QueryExpression { get; set; }
    }
}