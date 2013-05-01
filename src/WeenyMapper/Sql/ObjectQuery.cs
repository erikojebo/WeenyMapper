using System.Collections.Generic;

namespace WeenyMapper.Sql
{
    public class ObjectQuery
    {
        public ObjectQuery()
        {
            SubQueries = new List<AliasedObjectSubQuery>();
            Joins = new List<ObjectSubQueryJoin>();
        }

        public IList<AliasedObjectSubQuery> SubQueries { get; set; }
        public IList<ObjectSubQueryJoin> Joins { get; set; }
    }
}