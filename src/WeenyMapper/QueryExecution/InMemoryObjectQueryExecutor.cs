using System.Collections.Generic;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryObjectQueryExecutor : IObjectQueryExecutor
    {
        public string ConnectionString { get; set; }

        public IList<T> Find<T>(ObjectQuerySpecification querySpecification) where T : new()
        {
            throw new System.NotImplementedException();
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