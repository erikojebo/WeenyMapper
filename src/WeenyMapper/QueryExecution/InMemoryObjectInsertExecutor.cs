using System;
using System.Collections.Generic;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryObjectInsertExecutor : IObjectInsertExecutor
    {
        private readonly InMemoryDatabase _inMemoryDatabase;

        public InMemoryObjectInsertExecutor(InMemoryDatabase inMemoryDatabase)
        {
            _inMemoryDatabase = inMemoryDatabase;
        }

        public string ConnectionString { get; set; }

        public void Insert<T>(IEnumerable<T> entities)
        {
            _inMemoryDatabase.Entities<T>().AddRange(entities);
        }

        public void InsertAsync<T>(IEnumerable<T> entities, Action callback, Action<Exception> errorCallback = null)
        {
            throw new NotImplementedException();
        }
    }
}