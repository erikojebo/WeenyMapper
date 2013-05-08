using System;
using System.Collections.Generic;
using System.Reflection;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryObjectUpdateExecutor : IObjectUpdateExecutor
    {
        private readonly InMemoryDatabase _inMemoryDatabase;

        public InMemoryObjectUpdateExecutor(InMemoryDatabase inMemoryDatabase)
        {
            _inMemoryDatabase = inMemoryDatabase;
        }

        public string ConnectionString { get; set; }

        public void Update<T>(T instance)
        {
            _inMemoryDatabase.Update(instance);
        }

        public int Update<T>(QueryExpression queryExpression, IDictionary<PropertyInfo, object> setters)
        {
            return _inMemoryDatabase.Update<T>(queryExpression, setters);
        }
    }
}