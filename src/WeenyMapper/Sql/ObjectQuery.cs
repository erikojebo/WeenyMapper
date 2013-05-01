using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Exceptions;

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

        public AliasedObjectSubQuery GetSubQuery<T>()
        {
            var subQuery = SubQueries.FirstOrDefault(x => x.ResultType == typeof(T));

            if (subQuery == null)
                throw new WeenyMapperException("No sub query has been defined for the type '{0}'. Did you forget a Join?", typeof(T).FullName);

            return subQuery;
        }
    }
}