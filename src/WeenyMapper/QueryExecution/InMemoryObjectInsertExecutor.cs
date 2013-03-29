using System;
using System.Collections.Generic;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryObjectInsertExecutor : IObjectInsertExecutor
    {
        public string ConnectionString { get; set; }

        public void Insert<T>(IEnumerable<T> entities)
        {
            throw new NotImplementedException();
        }

        public void InsertAsync<T>(IEnumerable<T> entities, Action callback, Action<Exception> errorCallback = null)
        {
            throw new NotImplementedException();
        }
    }
}