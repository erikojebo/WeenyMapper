using System.Collections.Generic;

namespace WeenyMapper.Sql
{
    public class SqlQuery
    {
        public SqlQuery()
        {
            SubQueries = new List<AliasedSqlSubQuery>();
            Joins = new List<SqlSubQueryJoin>();
        }

        public List<AliasedSqlSubQuery> SubQueries { get; set; }
        public List<SqlSubQueryJoin> Joins { get; set; }
    }
}