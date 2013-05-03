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

        public AliasedObjectSubQuery GetOrCreateSubQuery<T>(string alias = null)
        {
            EnsureSubQuery<T>(alias);

            return GetSubQuery<T>(alias);
        }

        public AliasedObjectSubQuery GetSubQuery<T>(string alias = null)
        {
            return GetSubQuery(typeof(T), alias);
        }

        public AliasedObjectSubQuery GetSubQuery(Type type, string alias = null)
        {
            if (!HasSubQuery(type))
                throw new WeenyMapperException("No sub query has been defined for the type '{0}'. Did you forget a Join?", type.FullName);

            return SubQueries.First(x => x.ResultType == type && x.Alias == alias);
        }

        private bool HasSubQuery(Type type, string alias = null)
        {
            var subQueriesForType = SubQueries.Where(x => x.ResultType == type);

            return subQueriesForType.Any(x => x.Alias == alias);
        }

        public void AddJoin<TParent, TChild>(ObjectSubQueryJoin joinSpecification, string childAlias, string parentAlias)
        {
            EnsureSubQuery<TParent>(parentAlias);
            EnsureSubQuery<TChild>(childAlias);

            joinSpecification.ParentSubQuery = GetSubQuery<TParent>(parentAlias);
            joinSpecification.ChildSubQuery = GetSubQuery<TChild>(childAlias);

            joinSpecification.ChildSubQuery.Alias = childAlias;
            joinSpecification.ParentSubQuery.Alias = parentAlias;

            Joins.Add(joinSpecification);
        }

        private void EnsureSubQuery<T>(string alias)
        {
            if (!HasSubQuery(typeof(T), alias))
                CreateSubQuery<T>(alias);
        }

        private void CreateSubQuery<T>(string alias)
        {
            var subQuery = new AliasedObjectSubQuery(typeof(T))
                {
                    Alias = alias
                };

            SubQueries.Add(subQuery);
        }
    }
}