using System;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryObjectDeleteExecutor : IObjectDeleteExecutor
    {
        public string ConnectionString { get; set; }

        public int Delete<T>(T instance)
        {
            throw new NotImplementedException();
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