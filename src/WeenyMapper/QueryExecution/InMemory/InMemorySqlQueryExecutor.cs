using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Sql;

namespace WeenyMapper.QueryExecution.InMemory
{
    public class InMemorySqlQueryExecutor : ISqlQueryExecutor
    {
        private readonly InMemoryDatabase _inMemoryDatabase;

        public InMemorySqlQueryExecutor(InMemoryDatabase inMemoryDatabase)
        {
            _inMemoryDatabase = inMemoryDatabase;
        }

        public string ConnectionString { get; set; }

        public IList<T> Find<T>(SqlQuery sqlQuery) where T : new()
        {
            return _inMemoryDatabase.Find<T>(sqlQuery);
        }

        public TScalar FindScalar<T, TScalar>(SqlQuery sqlQuery)
        {
            return FindScalarList<T, TScalar>(sqlQuery).FirstOrDefault();
        }

        public IList<TScalar> FindScalarList<T, TScalar>(SqlQuery sqlQuery)
        {
            return _inMemoryDatabase.FindScalarList<T, TScalar>(sqlQuery);
        }
    }
}