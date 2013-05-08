using System;
using System.Collections.Generic;
using System.Linq;
using WeenyMapper.Async;

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
            _inMemoryDatabase.Add(entities);
        }

        public void InsertAsync<T>(IEnumerable<T> entities, Action callback, Action<Exception> errorCallback = null)
        {
            TaskRunner.Run(() => Insert(entities), callback, errorCallback);
        }
    }
}