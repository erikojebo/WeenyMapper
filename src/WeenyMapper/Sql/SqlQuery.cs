using System.Collections.Generic;
using System.Linq;

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

        public bool IsJoinQuery
        {
            get { return Joins.Any(); }
        }

        public void AddJoin(SqlSubQueryJoin joinSpec, string childAlias, string parentAlias)
        {
            joinSpec.ParentSubQuery = GetSubQuery(joinSpec.ParentTableName, parentAlias);
            joinSpec.ChildSubQuery = GetSubQuery(joinSpec.ChildTableName, childAlias);

            Joins.Add(joinSpec);
        }

        private AliasedSqlSubQuery GetSubQuery(string tableName, string alias)
        {
            if (!string.IsNullOrWhiteSpace(alias))
                return SubQueries.FirstOrDefault(x => x.Alias == alias);

            return SubQueries.FirstOrDefault(x => x.TableName == tableName);
        }
    }
}