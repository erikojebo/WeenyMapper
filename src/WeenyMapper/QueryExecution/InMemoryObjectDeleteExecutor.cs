using System;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryObjectDeleteExecutor : IObjectDeleteExecutor
    {
        private readonly InMemoryDatabase _inMemoryDatabase;

        public InMemoryObjectDeleteExecutor(InMemoryDatabase inMemoryDatabase)
        {
            _inMemoryDatabase = inMemoryDatabase;
        }

        public string ConnectionString { get; set; }

        public void Delete<T>(T instance)
        {
            _inMemoryDatabase.Delete(instance);
        }

        public void DeleteAsync<T>(T entity, Action callback, Action<Exception> errorCallback = null)
        {
            throw new NotImplementedException();
        }

        public int Delete<T>(QueryExpression queryExpression)
        {
            throw new NotImplementedException();
        }
    }
}