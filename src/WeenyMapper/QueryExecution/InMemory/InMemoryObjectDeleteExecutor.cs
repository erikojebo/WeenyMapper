using System;
using WeenyMapper.Async;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution.InMemory
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
            TaskRunner.Run(() => Delete(entity), callback, errorCallback);            
        }

        public int Delete<T>(QueryExpression queryExpression)
        {
            return _inMemoryDatabase.Delete<T>(queryExpression);
        }
    }
}