using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution.InMemory
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
            return _inMemoryDatabase.Find<T>(query);
        }

        public TScalar FindScalar<T, TScalar>(ObjectQuery query)
        {
            return FindScalarList<T, TScalar>(query).FirstOrDefault();
        }

        public IList<TScalar> FindScalarList<T, TScalar>(ObjectQuery query)
        {
            return _inMemoryDatabase.FindScalarList<T, TScalar>(query);
        }
    }
}