using System.Collections.Generic;
using System.Linq;
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

        public IList<T> Find<T>(ObjectQuery query) where T : new()
        {
            return _inMemoryDatabase.FindAll<T>();
        }

        public TScalar FindScalar<T, TScalar>(ObjectQuery query)
        {
            throw new System.NotImplementedException();
        }

        public IList<TScalar> FindScalarList<T, TScalar>(ObjectQuery query)
        {
            throw new System.NotImplementedException();
        }
    }
}