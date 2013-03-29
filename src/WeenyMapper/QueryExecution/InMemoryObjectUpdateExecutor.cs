using System.Collections.Generic;
using System.Reflection;
using WeenyMapper.QueryParsing;

namespace WeenyMapper.QueryExecution
{
    public class InMemoryObjectUpdateExecutor : IObjectUpdateExecutor
    {
        public string ConnectionString { get; set; }

        public int Update<T>(T instance)
        {
            throw new System.NotImplementedException();
        }

        public int Update<T>(QueryExpression queryExpression, IDictionary<PropertyInfo, object> setters)
        {
            throw new System.NotImplementedException();
        }
    }
}