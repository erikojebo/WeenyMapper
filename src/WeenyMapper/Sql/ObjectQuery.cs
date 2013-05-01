using System;
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
            return GetSubQuery(typeof(T));
        }

        public AliasedObjectSubQuery GetSubQuery(Type type)
        {
            if (!HasSubQuery(type))
                throw new WeenyMapperException("No sub query has been defined for the type '{0}'. Did you forget a Join?", type.FullName);

            return SubQueries.First(x => x.ResultType == type);
        }

        private bool HasSubQuery(Type type)
        {
            return SubQueries.Any(x => x.ResultType == type);
        }

        public void AddJoin<TParent, TChild>(ObjectSubQueryJoin joinSpecification)
        {
            EnsureSubQuery<TParent>();
            EnsureSubQuery<TChild>();

            joinSpecification.ParentSubQuery = GetSubQuery<TParent>();
            joinSpecification.ChildSubQuery = GetSubQuery<TChild>();

            Joins.Add(joinSpecification);
        }

        private void EnsureSubQuery<TParent>()
        {
            if (!HasSubQuery(typeof(TParent)))
                CreateSubQuery<TParent>();
        }

        private void CreateSubQuery<T>()
        {
            var subQuery = new AliasedObjectSubQuery(typeof(T));

            SubQueries.Add(subQuery);
        }
    }
}