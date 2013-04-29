using System.Collections.Generic;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryObjectQueryExecutor : IObjectQueryExecutor
    {
        private readonly InMemoryDatabase _inMemoryDatabase;

        public InMemoryObjectQueryExecutor(InMemoryDatabase inMemoryDatabase)
        {
            _inMemoryDatabase = inMemoryDatabase;
        }

        public string ConnectionString { get; set; }

        public IList<T> Find<T>(ObjectQuerySpecification querySpecification) where T : new()
        {
            querySpecification.QueryExpression
        }

        public TScalar FindScalar<T, TScalar>(ObjectQuerySpecification querySpecification)
        {
            throw new System.NotImplementedException();
        }

        public IList<TScalar> FindScalarList<T, TScalar>(ObjectQuerySpecification querySpecification)
        {
            throw new System.NotImplementedException();
        }
    }
}